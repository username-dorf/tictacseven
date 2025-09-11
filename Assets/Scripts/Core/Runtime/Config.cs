using PrimeTween;

namespace Core
{
    public class Config
    {
        public void Initialize()
        {
            PrimeTweenConfig.warnEndValueEqualsCurrent = false;
            PrimeTweenConfig.warnTweenOnDisabledTarget = false;
            PrimeTweenConfig.warnZeroDuration = false;
        }
    }
}