namespace mapa_back.Services
{
	public class CalculatorService
	{
		public double Add(double a, double b)
		{
			return a + b;
		}

		public double Subtract(double a, double b)
		{
			return a - b;
		}

		public double Multiply(double a, double b)
		{
			return a * b;
		}

		public double Divide(double a, double b)
		{
			if (b == 0)
			{
				throw new Exception("Nie można dzielić przez zero!");
			}

			return a / b;
		}
		public double Percent(double value, double percent)
		{
			return (value * percent) / 100.0;
		}
	}
}
