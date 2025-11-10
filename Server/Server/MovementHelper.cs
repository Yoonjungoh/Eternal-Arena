using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server
{
    public static class MovementHelper
    {
        public static Vector3 PVec3ToVec3(ProtoVector3 v) { return new Vector3(v.X, v.Y, v.Z); }

        public static Vector3 ForwardFrom(ProtoQuaternion q)
        {
            var quat = new Quaternion(q.X, q.Y, q.Z, q.W);
            // 3D 회전 적용
            var x2 = 2 * quat.X; var y2 = 2 * quat.Y; var z2 = 2 * quat.Z;
            var xx2 = quat.X * x2; var yy2 = quat.Y * y2; var zz2 = quat.Z * z2;
            var xy2 = quat.X * y2; var xz2 = quat.X * z2; var yz2 = quat.Y * z2;
            var wx2 = quat.W * x2; var wy2 = quat.W * y2; var wz2 = quat.W * z2;

            // (0,0,1)을 회전한 결과
            return new Vector3(
                x: xz2 + wy2,
                y: yz2 - wx2,
                z: 1 - (xx2 + yy2)
            );
        }

        public static Vector3 PredictPosition(Vector3 pos, Vector3 vel, long lastServerTime, long now)
        {
            float deltaSec = MathF.Max(0f, (now - lastServerTime) / 1000f);
            return pos + vel * deltaSec;
        }
    }
}
