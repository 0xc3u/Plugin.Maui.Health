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
		public static string MolesPerLiter = "mol<L";
		public static string MillimolesPerLiter = "mmol<L";
		public static string MilligramsPerDeciliter = "mg/dL";
		public static string InternationalUnits => "IU";
	}

	public static class Others
	{
		public static string Percentage => "%";
		public static string CountPerMinute => "count/min";
		public static string Count => "count";
	}
}


/*BodyMassIndex: No unit (it's a ratio)
BodyFatPercentage: Percent (%)
Height: Meters (m), Feet (ft), Centimeters (cm), Inches (in)
BodyMass: Kilograms (kg), Pounds (lb)
LeanBodyMass: Kilograms (kg), Pounds (lb)
HeartRate: Beats per Minute (bpm)
StepCount: Count
DistanceWalkingRunning: Meters (m), Kilometers (km), Feet (ft), Miles (mi)
DistanceCycling: Meters (m), Kilometers (km), Feet (ft), Miles (mi)
BasalEnergyBurned: Kilocalories (kcal), Kilojoules (kJ)
ActiveEnergyBurned: Kilocalories (kcal), Kilojoules (kJ)
FlightsClimbed: Count
NikeFuel: No unit (proprietary)
OxygenSaturation: Percent (%)
BloodGlucose: mg/dL, mmol/L
BloodPressureSystolic: mmHg
BloodPressureDiastolic: mmHg
BloodAlcoholContent: Percent (%)
PeripheralPerfusionIndex: Percent (%)
ForcedVitalCapacity: Liters (L)
ForcedExpiratoryVolume1: Liters (L)
PeakExpiratoryFlowRate: L/min
NumberOfTimesFallen: Count
InhalerUsage: Count
RespiratoryRate: Breaths per Minute
BodyTemperature: Degrees Celsius (°C), Degrees Fahrenheit (°F)
DietaryFatTotal: Grams (g)
DietaryFatPolyunsaturated: Grams (g)
DietaryFatMonounsaturated: Grams (g)
DietaryFatSaturated: Grams (g)
DietaryCholesterol: mg
DietarySodium: mg
DietaryCarbohydrates: Grams (g)
DietaryFiber: Grams (g)
DietarySugar: Grams (g)
DietaryEnergyConsumed: Kilocalories (kcal), Kilojoules (kJ)
DietaryProtein: Grams (g)
DietaryVitaminA: IU (International Units)
DietaryVitaminB6: mg
DietaryVitaminB12: µg (Micrograms)
DietaryVitaminC: mg
DietaryVitaminD: IU (International Units)
DietaryVitaminE: mg
DietaryVitaminK: µg (Micrograms)
DietaryCalcium: mg
DietaryIron: mg
DietaryThiamin: mg
DietaryRiboflavin: mg
DietaryNiacin: mg
DietaryFolate: µg (Micrograms)
DietaryBiotin: µg (Micrograms)
DietaryPantothenicAcid: mg
DietaryPhosphorus: mg
DietaryIodine: µg (Micrograms)
DietaryMagnesium: mg
DietaryZinc: mg
DietarySelenium: µg (Micrograms)
DietaryCopper: mg
DietaryManganese: mg
DietaryChromium: µg (Micrograms)
DietaryMolybdenum: µg (Micrograms)
DietaryChloride: mg
DietaryPotassium: mg
DietaryCaffeine: mg
UnderwaterDepth: Meters (m)
WaterTemperature: Degrees Celsius (°C), Degrees Fahrenheit (°F)*/