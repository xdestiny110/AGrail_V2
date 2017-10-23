using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Framework.Network
{
    [XLua.LuaCallCSharp]
    public sealed class TCP
    {
        private ICoder coder;
        private ServerConfig serverConfig;
        private Socket socket;
        private bool startReconnect;
        private int reconnectDelay;
        private int reconnectDelayMin = 1000;
        private int reconnectDelayMax = 60000;
        private bool autoReconnect;
        private int reconnectTimes = 0;
        private int maxReconnectTimes = 3;

        private readonly ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

        public bool Connected { get { return null != socket && socket.Connected; } }

        public bool AutoReconnect
        {
            get { return autoReconnect; }
            set
            {
                autoReconnect = value;
                if (autoReconnect)
                {
                    if (!Connected)
                    {
                        reconnectDelay = 0;
                        startReconnect = true;
                        closeAndReconnect(socket);
                    }
                }
                else
                    startReconnect = false;
            }
        }

        public TCP(ServerConfig serverConfig, ICoder coder, bool reconnect = true, int maxReconnectTimes = 3)
        {
            this.serverConfig = serverConfig;
            this.coder = coder;
            autoReconnect = reconnect;
            this.maxReconnectTimes = maxReconnectTimes;

            MonoRoot.Instance.AddUpdateAction(() =>
            {
                while (actions.Count > 0)
                    actions.Dequeue()();
            });
        }

        public void Connect()
        {
            if(socket != null)
            {
                UnityEngine.Debug.LogWarning("Socket has been established!");
                return;
            }

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    SendBufferSize = serverConfig.SendBufferSize,
                    ReceiveBufferSize = serverConfig.ReceiveBufferSize
                };
                socket.BeginConnect(serverConfig.ServerIP, serverConfig.Port,
                    (ar) =>
                    {
                        try
                        {
                            socket.EndConnect(ar);
                            onConnectSuccess();
                            StateObject state = new StateObject() { workSocket = (Socket)ar.AsyncState };
                            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, beginReceiveCallback, state);
                            reconnectDelay = 0;
                            startReconnect = false;
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogWarning("TCP.Connect failed:");
                            UnityEngine.Debug.LogException(ex);
                            closeAndReconnect(socket);
                        }
                    }, socket);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogWarning("TCP.Connect failed :");
                UnityEngine.Debug.LogException(ex);
            }
        }

        public void Close()
        {
            if(socket!=null && socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            actions.clear();
            startReconnect = false;
            UnityEngine.Debug.Log("Disconnect.");
        }

        public void Send(Protobuf protobuf)
        {
            if(socket == null || !socket.Connected)
            {
                UnityEngine.Debug.LogError("Socket was not established!");
                return;
            }
#if LOGON
            UnityEngine.Debug.Log(coder.PrintContent(protobuf));
#endif
            beforeSendProto(protobuf);
            var data = constructPacket(coder.Encode(protobuf), (short)protobuf.ProtoID);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, beginSendCallback, socket);
        }

        private void closeAndReconnect(Socket socket)
        {
            if (socket != this.socket)
                return;

            Close();
            if (autoReconnect && reconnectTimes < maxReconnectTimes)
            {
                if (reconnectDelay == 0)
                {
                    reconnectDelay = reconnectDelayMin;
                }
                else
                {
                    reconnectDelay *= 2;
                    if (reconnectDelay > reconnectDelayMax)
                        reconnectDelay = reconnectDelayMax;
                }
                startReconnect = true;
                reconnectTimes++;
                Connect();
                actions.Enqueue(onReconnect);
            }
            else
                actions.Enqueue(onConnectFail);
        }

        private MemoryStream stream = new MemoryStream();
        private BinaryReader br;
        private void beginReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket socket = state.workSocket;
            if (br == null)
                br = new BinaryReader(stream);

            try
            {
                int readLength = socket.EndReceive(ar);
                if(readLength > 0)
                {
                    UnityEngine.Debug.Log("TCP Receive data length = " + readLength);
                    // 处理粘包、分包等问题
                    // 新来的数据写入stream末尾
                    stream.Position = stream.Length;
                    stream.Write(state.buffer, 0, readLength);
                    stream.Position = 0;
                    List<Protobuf> proto = deconstructPacket();
                    foreach (var v in proto)
                        afterReceiveProto(v);
                }
                // 继续接收
                socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, beginReceiveCallback, state);
            }
            catch(SocketException ex)
            {
                UnityEngine.Debug.LogWarning("TCP.Receive failed :");
                UnityEngine.Debug.LogException(ex);
            }
        }

        private void beginSendCallback(IAsyncResult ar)
        {
            try
            {
                var sock = (Socket)ar.AsyncState;
                sock.EndSend(ar);
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.LogError("TCP.beginSend failed: ");
                UnityEngine.Debug.LogException(ex);
            }
        }

        private void onConnectSuccess()
        {
            actions.Enqueue(() => {
                UnityEngine.Debug.Log("OnConnectSuccess!");
                Message.MessageSystem.Notify("OnConnect", true);
            });
        }

        private void onConnectFail()
        {
            actions.Enqueue(() =>
            {
                UnityEngine.Debug.Log("OnConnectFail!");
                Message.MessageSystem.Notify("OnConnect", false);
            });
        }

        private void onDisconnect()
        {
            actions.Enqueue(() =>
            {
                UnityEngine.Debug.Log("OnDisconnect!");
                Message.MessageSystem.Notify("OnDisconnect");
            });
        }

        private void onReconnect()
        {
            actions.Enqueue(() =>
            {
                UnityEngine.Debug.Log("OnDisconnect!");
                Message.MessageSystem.Notify("OnReconnect");
            });
        }

        private void beforeSendProto(Protobuf protobuf)
        {
            UnityEngine.Debug.LogFormat("Send protobuf {0}", protobuf.ProtoID);
        }

        private void afterReceiveProto(Protobuf protobuf)
        {
            actions.Enqueue(() =>
            {
                UnityEngine.Debug.LogFormat("Recv protobuf {0}", protobuf.ProtoID);
                Message.MessageSystem.Notify(protobuf.ProtoID.ToString(), protobuf.Proto);
            });
        }

        private byte[] constructPacket(byte[] encode, short protobufID)
        {
            // 整个数据流结构为
            // 4个字节的包长度(不算在长度中), 2个字节的包长度, 2个字节的请求类型
            MemoryStream sendStream = new MemoryStream();
            sendStream.SetLength(encode.Length + 8);
            sendStream.Position = 0;
            sendStream.Write(BitConverter.GetBytes(encode.Length + 4), 0, 4);
            sendStream.Write(BitConverter.GetBytes((short)encode.Length + 4), 0, 2);
            sendStream.Write(BitConverter.GetBytes(protobufID), 0, 2);
            sendStream.Write(encode, 0, encode.Length);
            var res = new byte[sendStream.Length];
            Array.Copy(sendStream.GetBuffer(), res, sendStream.Length);
            return res;
        }

        private List<Protobuf> deconstructPacket()
        {
            var ret = new List<Protobuf>();
            // 4个字节的协议长度
            var sumLen = br.ReadInt32();
            while (sumLen + 4 <= stream.Length)
            {
                // 2个字节协议号
                stream.Position = 6;
                var protoID = br.ReadInt16();
                // 协议反序列化
                var protoStream = new MemoryStream();
                protoStream.Write(stream.ToArray(), 8, sumLen - 4);
                protoStream.Position = 0;
                ret.Add(new Protobuf() { Proto = coder.Decode(protoStream.GetBuffer(), protoID), ProtoID = protoID });
#if LOGON
                UnityEngine.Debug.Log(coder.PrintContent(ret[ret.Count - 1]));
#endif
                // 判断黏包
                stream.Position = sumLen + 4;
                if (stream.Length - stream.Position > 0)
                {
                    MemoryStream newStream = new MemoryStream();
                    newStream.Write(stream.ToArray(), (int)stream.Position, (int)(stream.Length - stream.Position));
                    stream = newStream;
                    br = new BinaryReader(stream);
                    // 判断剩下的是否包含一个完整的协议
                    stream.Position = 0;
                    sumLen = br.ReadInt32();
                }
                else
                {
                    // 说明说是一个完整的包
                    stream = new MemoryStream();
                    br = new BinaryReader(stream);
                    break;
                }
            }
            return ret;
        }
    }

    public class StateObject
    {
        // Client  socket.
        public Socket workSocket;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public MemoryStream stream = new MemoryStream();
    }

}


