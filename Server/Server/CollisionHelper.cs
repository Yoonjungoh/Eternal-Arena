using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server
{
    public static class CollisionHelper
    {
        // 수평 부채꼴: XZ 평면만 사용 + 높이 허용
        public static bool IsInHorizontalSector(Vector3 origin, Vector3 forward, Vector3 target,
                                         float radius, float cosLimit, float height,
                                         out float sqrDistXZ)
        {
            Vector3 v = target - origin;

            // 높이 제한 (y 축)
            if (MathF.Abs(v.Y) > height * 0.5f) { sqrDistXZ = 0; return false; }

            v.Y = 0;                     // 수평 성분
            forward.Y = 0;
            sqrDistXZ = v.LengthSquared();
            if (sqrDistXZ > radius * radius) return false;

            if (sqrDistXZ < 1e-6f) return true; // 같은 위치면 포함

            Vector3 vN = Vector3.Normalize(v);
            Vector3 fN = Vector3.Normalize(forward);
            float cosTheta = Vector3.Dot(vN, fN);
            return cosTheta >= cosLimit;
        }
    }
}
