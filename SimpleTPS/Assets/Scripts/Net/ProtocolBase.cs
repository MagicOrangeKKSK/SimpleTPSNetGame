//协议基类
public class ProtocolBase
{
    //通用的接口  解码  编码  获取协议名称 打印输出的秒速

    //解码
    public virtual ProtocolBase Decode(byte[] readbuff, int start, int length)
    {
        return new ProtocolBase();
    }

    //编码器
    public virtual byte[] Encode()
    {
        return new byte[] { };
    }

    //获取协议名字
    public virtual string GetName()
    {
        return "";
    }

    //描述
    public virtual string GetDesc()
    {
        return "";
    }
}