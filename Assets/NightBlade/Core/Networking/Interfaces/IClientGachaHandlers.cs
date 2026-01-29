using LiteNetLibManager;

namespace NightBlade
{
    public interface IClientGachaHandlers
    {
        bool RequestGachaInfo(ResponseDelegate<ResponseGachaInfoMessage> callback);
        bool RequestOpenGacha(RequestOpenGachaMessage data, ResponseDelegate<ResponseOpenGachaMessage> callback);
    }
}







