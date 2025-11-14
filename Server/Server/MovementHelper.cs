using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Server
{
    public static class MovementHelper
    {
        public static Vector3 ProtoVec3ToVec3(ProtoVector3 v) { return new Vector3(v.X, v.Y, v.Z); }
        public static ProtoVector3 Vec3ToProtoVec3(Vector3 v) { return new ProtoVector3 { X = v.X, Y = v.Y, Z = v.Z }; }

        public static Vector3 ForwardFrom(ProtoQuaternion q)
        {
            // 행렬 곱셈
            Quaternion quat = new Quaternion(q.X, q.Y, q.Z, q.W);
            return Vector3.Transform(Vector3.UnitZ, quat); // (0,0,1) 회전 적용
        }

        public static Vector3 PredictPosition(Vector3 pos, Vector3 vel, long lastServerTime, long now)
        {
            float deltaSec = MathF.Max(0f, (now - lastServerTime) / 1000f);
            return pos + vel * deltaSec;
        }
    }
}
