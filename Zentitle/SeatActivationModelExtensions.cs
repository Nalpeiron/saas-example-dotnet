namespace ZentitleSaaSDemo.Zentitle;

public static class SeatActivationModelExtensions
{
    internal static bool IsAccessTokenValid(this ActivationModel model)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        return model.LeaseExpiry > DateTime.UtcNow;
    }
    
    internal static TimeSpan GetSeatExpiresIn(this ActivationModel model, int offset = 0)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));
        return model.LeaseExpiry - DateTime.UtcNow.AddMinutes(offset);
    }
}