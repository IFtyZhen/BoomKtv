package top.icelery.config;

import lombok.Data;
import org.springframework.boot.context.properties.ConfigurationProperties;
import top.icelery.entity.User;

import java.util.Map;

@Data
@ConfigurationProperties(prefix = "app")
public class BoomKtvProperties {

    private int port;
    private String key;
    private Map<String, User> users;

}
