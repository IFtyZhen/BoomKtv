package top.icelery;

import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.context.properties.ConfigurationPropertiesScan;
import org.springframework.scheduling.annotation.EnableScheduling;
import top.icelery.config.BoomKtvProperties;
import top.icelery.manager.NetworkManager;
import top.icelery.network.NettyServer;

@EnableScheduling
@ConfigurationPropertiesScan
@SpringBootApplication
public class BoomKtvApplication implements CommandLineRunner {

    public static BoomKtvProperties boomKtvProp;

    public static NetworkManager networkManager;

    public BoomKtvApplication(BoomKtvProperties boomKtvProp, NetworkManager networkManager) {
        BoomKtvApplication.boomKtvProp = boomKtvProp;
        BoomKtvApplication.networkManager = networkManager;
    }

    @Override
    public void run(String... args) {
        try {
            new NettyServer(boomKtvProp.getPort());
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }

    public static void main(String[] args) {
        SpringApplication.run(BoomKtvApplication.class, args);
    }

}
