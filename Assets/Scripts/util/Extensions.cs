using UnityEngine;
using System;

public static class Extensions {

	public static Vector2 ToVector2(this Vector3 v) {
		return new Vector2(v.x, v.y);
	}

	public static Vector2 AsVector(this Road r) {
		return new Vector2(r.xPos, r.yPos);
	}

	public static bool AlmostEqual(this Vector2 vec, Vector2 other) {
		return Vector2.SqrMagnitude(vec - other) < 0.001f;
	}

	public static bool AlmostEqual(this Vector3 vec, Vector3 other) {
		return Vector3.SqrMagnitude(vec - other) < 0.001f;
	}

	public static Vector2 Swap(this Vector2 vec) {
		return new Vector2(vec.y, vec.x);
	}

	public static Vector3 ToVector3(this Vector2 v) {
		return new Vector3(v.x, v.y, 0f);
	}

	public static float Distance(this Vector2 a, Vector2 b) {
		return Mathf.Sqrt((a.x-b.x)*(a.x-b.x)+(a.y-b.y)*(a.y-b.y));
	}

	public static float Distance(this Vector3 a, Vector3 b) {
		return Mathf.Sqrt((a.x-b.x)*(a.x-b.x)+(a.y-b.y)*(a.y-b.y));
	}

	public static bool Almost(this float a, float b) {
		return Mathf.Abs(a-b) < 0.02f;
	}

	/*public static Vector2 Lerp(this Vector2 from, Vector2 to, float t) {
		return new Vector2()
	} */

	public static bool Intersects(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2) {
		Vector2 cmp = new Vector2(b1.x - a1.x, b1.y - a1.y);
		Vector2 r = new Vector2(a2.x - a1.x, a2.y - a1.y);
		Vector2 s = new Vector2(b2.x - b1.x, b2.y - b2.y);

		float cmpxr = cmp.x * r.y - cmp.y * r.x;
		float cmpxs = cmp.x * s.y - cmp.y * s.x;
		float rxs = r.x * s.y - r.y * s.x;

		if (cmpxr == 0f) {
			return ((b1.x - a1.x < 0f) != (b1.x - a2.x < 0f))
				|| ((b1.y - a1.y < 0f) != (b1.y - a2.y < 0f));
		}

		if (rxs == 0f)
			return false;

		float rxsr = 1f;
		float t = cmpxs * rxsr;
		float u = cmpxr * rxsr;

		return (t >= 0f) && (t <= 1f) && (u >= 0f) && (u <= 1f);
	}

}
