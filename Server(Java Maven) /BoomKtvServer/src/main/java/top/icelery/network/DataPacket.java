package top.icelery.network;

import io.netty.buffer.ByteBuf;
import io.netty.buffer.Unpooled;
import io.netty.channel.Channel;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.nio.charset.StandardCharsets;

public class DataPacket {

    private static final Logger logger = LoggerFactory.getLogger(DataPacket.class);

    private Channel channel;

    private final ByteBuf byteBuf;

    private Opcode opcode;

    public Opcode getOpcode() {
        return opcode;
    }

    private DataPacket(ByteBuf byteBuf) {
        this.byteBuf = byteBuf;
    }

    public void flush() {
        logger.info("sendPack opcode={}", opcode);
        channel.writeAndFlush(byteBuf);
    }

    public static DataPacket allocate(Channel channel, Opcode opcode) {
        DataPacket packet = new DataPacket(Unpooled.buffer());
        packet.channel = channel;
        packet.opcode = opcode;
        return packet.writeShort(opcode.ordinal());
    }

    public static DataPacket parse(Channel channel, ByteBuf buf) {
        DataPacket packet = new DataPacket(buf);
        packet.channel = channel;
        packet.opcode = Opcode.values()[packet.readShort()];
        return packet;
    }

    public DataPacket replyPack(Opcode opcode) {
        return DataPacket.allocate(channel, opcode);
    }

    public ByteBuf getByteBuf() {
        return byteBuf;
    }

    public byte readByte() {
        return byteBuf.readByte();
    }

    public void readBytes(byte[] v) {
        byteBuf.readBytes(v);
    }

    public short readShort() {
        return byteBuf.readShort();
    }

    public int readInt() {
        return byteBuf.readInt();
    }

    public long readLong() {
        return byteBuf.readLong();
    }

    public float readFloat() {
        return byteBuf.readFloat();
    }

    public double readDouble() {
        return byteBuf.readDouble();
    }

    public char readChar() {
        return byteBuf.readChar();
    }

    public String readString() {
        byte[] bytes = new byte[byteBuf.readInt()];
        byteBuf.readBytes(bytes);
        return new String(bytes, StandardCharsets.UTF_8);
    }

    public DataPacket writeByte(byte v) {
        byteBuf.writeByte(v);
        return this;
    }

    public DataPacket writeBytes(byte[] v) {
        byteBuf.writeBytes(v);
        return this;
    }

    public DataPacket writeShort(int v) {
        byteBuf.writeShort(v);
        return this;
    }

    public DataPacket writeInt(int v) {
        byteBuf.writeInt(v);
        return this;
    }

    public DataPacket writeLong(long v) {
        byteBuf.writeLong(v);
        return this;
    }

    public DataPacket writeFloat(float v) {
        byteBuf.writeFloat(v);
        return this;
    }

    public DataPacket writeDouble(double v) {
        byteBuf.writeDouble(v);
        return this;
    }

    public DataPacket writeChar(char v) {
        byteBuf.writeChar(v);
        return this;
    }

    public DataPacket writeString(String v) {
        byte[] bytes = v.getBytes(StandardCharsets.UTF_8);
        byteBuf.writeInt(bytes.length);
        byteBuf.writeBytes(bytes);
        return this;
    }

}
