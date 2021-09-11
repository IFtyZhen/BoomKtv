package top.icelery.manager;

import io.netty.buffer.ByteBuf;
import io.netty.channel.Channel;
import io.netty.channel.ChannelId;
import io.netty.channel.group.ChannelGroup;
import io.netty.channel.group.DefaultChannelGroup;
import io.netty.util.concurrent.GlobalEventExecutor;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import top.icelery.BoomKtvApplication;
import top.icelery.config.BoomKtvProperties;
import top.icelery.entity.User;
import top.icelery.network.DataPacket;
import top.icelery.network.Opcode;

import java.util.Collection;
import java.util.HashMap;
import java.util.Map;

@Component
public class NetworkManager {

    public final Map<ChannelId, User> onlineUsers;

    private static final Logger logger = LoggerFactory.getLogger(NetworkManager.class);

    public final ChannelGroup channels = new DefaultChannelGroup(GlobalEventExecutor.INSTANCE);

    public NetworkManager() {
        this.onlineUsers = new HashMap<>();
    }

    public void addChannel(Channel channel) {
        logger.info("AddChannel {}", channel.id());
        channels.add(channel);
    }

    public void removeChannel(Channel channel) {
        logger.info("RemoveChannel {}", channel.id());
        channels.remove(channel);
        onUserOffline(channel.id());
    }

    public void channelRead(Channel channel, ByteBuf byteBuf) {
        logger.info("channelRead PackSize={}", byteBuf.readableBytes());
        DataPacket packet = DataPacket.parse(channel, byteBuf);

        if (packet.getOpcode() == Opcode.CsAuth) {
            String uid = packet.readString();
            String key = packet.readString();
            NetworkManager.AuthResult result = auth(uid, key);

            if (result == NetworkManager.AuthResult.Success) {
                onUserOnline(channel.id(), BoomKtvApplication.boomKtvProp.getUsers().get(uid));
            }

            DataPacket sendPack = packet.replyPack(Opcode.CsAuth);
            sendPack.writeShort(result.ordinal());
            sendPack.flush();

        } else if (onlineUsers.get(channel.id()) != null) {
            handleNetworkPacket(packet);
        }
    }

    public void readComplete(Channel channel) {
        if (onlineUsers.get(channel.id()) == null) {
            channel.close();
        }
    }

    private void onUserOnline(ChannelId channelId, User user) {
        logger.info("onUserOnline {}", user);
        DataPacket packet = DataPacket.allocate(null, Opcode.SEnter);
        packet.writeString(user.getUid());
        packet.writeString(user.getName());
        sendToAll(packet);
        onlineUsers.put(channelId, user);
    }

    private void onUserOffline(ChannelId channelId) {
        User user = onlineUsers.remove(channelId);
        if (user != null) {
            logger.info("onUserOffline {}", user);
            DataPacket packet = DataPacket.allocate(null, Opcode.SExit);
            packet.writeString(user.getUid());
            packet.writeString(user.getName());
            sendToAll(packet);
        }
    }

    private enum AuthResult {
        Success,
        KeyErr,
        UidNotFound,
        UserIsOnline
    }

    private AuthResult auth(String uid, String key) {
        BoomKtvProperties boomKtvProp = BoomKtvApplication.boomKtvProp;

        if (!boomKtvProp.getKey().equals(key)) {
            return AuthResult.KeyErr;
        }

        if (boomKtvProp.getUsers().get(uid) == null) {
            return AuthResult.UidNotFound;
        }

        for (User user : onlineUsers.values()) {
            if (user.getUid().equals(uid)) {
                return AuthResult.UserIsOnline;
            }
        }

        return AuthResult.Success;
    }

    private void handleNetworkPacket(DataPacket packet) {
        logger.info("HandleNetworkPacket {}", packet.getOpcode());
        switch (packet.getOpcode()) {
            case CsHeartbeat:
//                packet.replyPack(Opcode.CsHeartbeat).flush();
                break;
            case CsUserList:
                handleUserList(packet);
                break;
            case CsRecord:
                handleRecord(packet);
                break;
        }
    }

    private void handleUserList(DataPacket packet) {
        DataPacket sendPack = packet.replyPack(Opcode.CsUserList);
        Collection<User> users = onlineUsers.values();
        sendPack.writeShort(users.size());
        for (User user : users) {
            sendPack.writeString(user.getUid());
            sendPack.writeString(user.getName());
        }
        sendPack.flush();
    }

    private void handleRecord(DataPacket packet) {
        DataPacket sendPack = packet.replyPack(Opcode.CsRecord);
        int len = packet.readInt();
        for (int i = 0; i < len; i++) {
            sendPack.writeByte(packet.readByte());
        }

        sendToAll(sendPack);
    }

    private void sendToAll(DataPacket packet) {
        ByteBuf buf = packet.getByteBuf();
        for (ChannelId id : onlineUsers.keySet()) {
            Channel channel = channels.find(id);
            if (channel != null) {
                channel.writeAndFlush(buf.copy());
            }
        }
    }

}
