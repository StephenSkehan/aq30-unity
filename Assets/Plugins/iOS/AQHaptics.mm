#import <UIKit/UIKit.h>

// Minimal impact-haptics bridge. Style: 0 = light, 1 = medium, 2 = heavy.
extern "C" void _aqHapticImpact(int style)
{
    if (@available(iOS 10.0, *))
    {
        UIImpactFeedbackStyle s;
        switch (style)
        {
            case 2:  s = UIImpactFeedbackStyleHeavy;  break;
            case 1:  s = UIImpactFeedbackStyleMedium; break;
            default: s = UIImpactFeedbackStyleLight;  break;
        }
        UIImpactFeedbackGenerator *gen = [[UIImpactFeedbackGenerator alloc] initWithStyle:s];
        [gen prepare];
        [gen impactOccurred];
    }
}
