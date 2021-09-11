namespace Transport
{
    public enum Opcode
    {
        CsAuth,
        CsHeartbeat,
        CsUserList,
        SEnter,
        SExit,
        CsRecord
    }
    
    public enum AuthResult {
        Success,
        KeyErr,
        UidNotFound,
        UserIsOnline
    }
}