namespace Plugin.Maui.Health.Constants;

public static class Units
{
	public static class Length
	{
		public static string Meters => "m";
		public static string Feet => "ft";
		public static string Centimeters => "cm";
		public static string Inches => "in";
		public static string Miles => "mi";
		public static string Kilometers => "km";
	}

	public static class Mass
	{
		public static string Milligrams => "mg";
		public static string Grams => "g";
		public static string Kilograms => "kg";
		public static string Pounds => "kg";
		public static string Ounces => "oz";
	}

	public static class Volume
	{
		public static string Liters => "l";
		public static string Milliliters => "mL";
		public static string FluidOunces => "floz";
		public static string Pints => "pt";
		public static string Quarts => "qt";
		public static string Gallons => "gal";
	}

	public static class Energy
	{
		public static string Kilocalories => "kcal";
		public static string Calories => "cal";
		public static string Joules => "j";
		public static string Kilojoules => "kJ";
	}

	public static class Temperature
	{
		public static string DegreesCelsius => "degC";
		public static string DegreesFahrenheit => "degF";
	}

	public static class Time
	{
		public static string Seconds => "s";
		public static string Minutes => "min";
		public static string Hours => "hr";
		public static string Days => "d";
	}

	public static class Pressure
	{
		public static string MillimetersOfMercury => "mmHg";
		public static string CentimetersOfWater => "cmH2O";
		public static string Atmospheres => "atm";
		public static string Pascals => "Pa";
	}

	public static class Concentration
	{
		public static string MolesPerLiter => "mol<L";
		public static string MillimolesPerLiter => "mmol<L";
		public static string MilligramsPerDeciliter => "mg/dL";
		public static string InternationalUnits => "IU";
		public static string MillilitersPerKilogramPerMinute => "ml/kg/min";
	}

	public static class Others
	{
		public static string Percentage => "%";
		public static string CountPerMinute => "count/min";
		public static string Count => "count";
	}
}