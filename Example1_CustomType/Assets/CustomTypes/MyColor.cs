using UnityEngine;

#pragma warning disable 659, 661		// disable：== Equals and GetHashCode

public struct MyColor
{
	public static MyColor white = new MyColor(1.0f, 1.0f, 1.0f, 1.0f);
	public static MyColor gray = new MyColor(0.5f, 0.5f, 0.5f, 0.5f);
	public float r;
	public float g;
	public float b;
    public float a;

	public static bool operator ==(MyColor c1, MyColor c2)
	{
		return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
	}

	public static bool operator !=(MyColor c1, MyColor c2)
	{
		return !(c1 == c2);
	}

	public override bool Equals(object c)
	{
		if (c == null)
			return false;

		if (c is MyColor)
			return this == (MyColor)c;
		return false;
	}

	public MyColor(float r, float g, float b, float a)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = a;
	}

	public MyColor(float r, float g, float b)
	{
		this.r = r;
		this.g = g;
		this.b = b;
		this.a = 1f;
	}

	public static implicit operator Color(MyColor c)
	{
		return new Color(c.r, c.g, c.b, c.a);
	}

	public static implicit operator MyColor(Color c)
	{
		return new MyColor(c.r, c.g, c.b, c.a);
	}
}
