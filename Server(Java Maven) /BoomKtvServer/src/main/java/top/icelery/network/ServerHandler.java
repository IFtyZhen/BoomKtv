package top.icelery.network;

import io.netty.buffer.ByteBuf;
import io.netty.channel.ChannelHandlerContext;
import io.netty.channel.ChannelInboundHandlerAdapter;
import io.netty.handler.timeout.IdleState;
import io.netty.handler.timeout.IdleStateEvent;
import top.icelery.BoomKtvApplication;
import top.icelery.manager.NetworkManager;

public class ServerHandler extends ChannelInboundHandlerAdapter {

    private final NetworkManager networkManager;

    public ServerHandler() {
        this.networkManager = BoomKtvApplication.networkManager;
    }

    @Override
    public void handlerAdded(ChannelHandlerContext ctx) {
        networkManager.addChannel(ctx.channel());
    }

    @Override
    public void handlerRemoved(ChannelHandlerContext ctx) {
        networkManager.removeChannel(ctx.channel());
    }

    @Override
    public void channelRead(ChannelHandlerContext ctx, Object msg) {
        networkManager.channelRead(ctx.channel(), (ByteBuf) msg);
    }

    @Override
    public void channelReadComplete(ChannelHandlerContext ctx) {
        networkManager.readComplete(ctx.channel());
    }

    @Override
    public void userEventTriggered(ChannelHandlerContext ctx, Object evt) {
        IdleStateEvent event = (IdleStateEvent) evt;
        if (IdleState.READER_IDLE.equals(event.state())) {
            ctx.channel().close();
        }
    }

}
