using Plugin.Maui.Health.Enums;
using Plugin.Maui.Health.Models;

namespace Plugin.Maui.Health;

public interface IHealth
{
	/// <summary>Returns true if the health data store is available on this device.</summary>
	bool IsSupported { get; }

	/// <summary>
	/// Checks (and requests, on iOS) authorization for the given health parameter and permission type.
	/// </summary>
	/// <returns>True if the required permissions are granted.</returns>
	/// <remarks>
	/// On Android, this method only checks whether permissions are already granted — it does NOT launch
	/// the Health Connect consent UI. The consuming app must register the
	/// <c>ACTION_SHOW_PERMISSIONS_RATIONALE</c> intent filter and drive the consent flow separately.
	/// </remarks>
	Task<bool> CheckPermissionAsync(HealthParameter healthParameter, PermissionType permissionType,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Checks whether authorization for <b>all</b> of the given parameters is currently granted, in a
	/// single call. A convenience batch companion to <see cref="CheckPermissionAsync"/>.
	/// </summary>
	/// <param name="requests">The parameters and the read/write access to check for each.</param>
	/// <returns>True only if every requested permission is granted.</returns>
	/// <remarks>
	/// Like <see cref="CheckPermissionAsync"/>, this never prompts the user — on Android it only queries
	/// already-granted state (it does not launch the Health Connect consent UI); on iOS checking and
	/// requesting are the same HealthKit operation. Parameters not supported by a platform are skipped
	/// rather than failing the whole check.
	/// </remarks>
	Task<bool> CheckPermissionsAsync(
		IEnumerable<(HealthParameter healthParameter, PermissionType permissionType)> requests,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests authorization for the given health parameter and permission type by presenting the
	/// platform's consent UI, then reports whether the permission ends up granted. This is the
	/// cross-platform way to obtain consent — the plugin maps the parameter to the correct native
	/// permissions on each platform, so consumers do not need to register their own launcher or
	/// re-implement the permission mapping.
	/// </summary>
	/// <returns>True if the required permissions are granted after the request completes.</returns>
	/// <remarks>
	/// On iOS, presents the HealthKit authorization sheet. HealthKit never reveals read-grant status,
	/// so a read request returns <c>true</c> once the sheet has been shown; only write denials are
	/// detectable.
	/// On Android, if the permissions are already granted this returns <c>true</c> without prompting;
	/// otherwise it launches the Health Connect consent UI using the current foreground
	/// <c>Activity</c> (which must derive from AndroidX <c>ComponentActivity</c>, e.g.
	/// <c>MauiAppCompatActivity</c>) and re-checks after the user returns. The app must still declare
	/// the privacy-policy rationale entry points in its manifest (see the README).
	/// </remarks>
	Task<bool> RequestPermissionAsync(HealthParameter healthParameter, PermissionType permissionType,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests authorization for several parameters in a single consent prompt and reports whether all
	/// of them end up granted. Prefer this over multiple <see cref="RequestPermissionAsync"/> calls:
	/// requesting permissions one-by-one shows several consecutive prompts, and on iOS presenting more
	/// than one HealthKit authorization sheet in quick succession can hang the app.
	/// </summary>
	/// <param name="requests">The parameters and the read/write access requested for each.</param>
	/// <returns>True if every requested permission is granted after the prompt completes.</returns>
	/// <remarks>
	/// On iOS this maps to a single <c>HKHealthStore.RequestAuthorizationToShareAsync</c> call (one
	/// sheet); read grants aren't revealed by HealthKit, so read requests count as granted once the
	/// sheet is shown. On Android the Health Connect consent UI is launched once for the combined set
	/// of permissions (and skipped entirely if they are already granted). Parameters not supported by a
	/// platform are ignored rather than failing the whole request.
	/// </remarks>
	Task<bool> RequestPermissionsAsync(
		IEnumerable<(HealthParameter healthParameter, PermissionType permissionType)> requests,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the cumulative sum of <paramref name="healthParameter"/> over the specified date range.
	/// Only meaningful for inherently cumulative parameters (steps, distance, energy, floors).
	/// </summary>
	/// <returns>The cumulative sum, or 0 if no data exists in the range.</returns>
	Task<double> ReadCountAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the most recent sample of <paramref name="healthParameter"/> in the specified date range.
	/// </summary>
	/// <param name="unit">The unit to express the value in. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>The latest value, or null if no sample exists in the range.</returns>
	Task<double?> ReadLatestAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the most recent sample of <paramref name="healthParameter"/> ever recorded, regardless of date.
	/// </summary>
	/// <param name="unit">The unit to express the value in. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>The latest <see cref="Sample"/>, or null if no data exists.</returns>
	Task<Sample?> ReadLatestAvailableAsync(HealthParameter healthParameter, string unit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the arithmetic mean of all samples of <paramref name="healthParameter"/> in the specified date range.
	/// </summary>
	/// <param name="unit">The unit to express values in. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>The average value, or null if no samples exist in the range.</returns>
	Task<double?> ReadAverageAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the minimum value among all samples of <paramref name="healthParameter"/> in the specified date range.
	/// </summary>
	/// <param name="unit">The unit to express values in. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>The minimum value, or null if no samples exist in the range.</returns>
	Task<double?> ReadMinAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the maximum value among all samples of <paramref name="healthParameter"/> in the specified date range.
	/// </summary>
	/// <param name="unit">The unit to express values in. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>The maximum value, or null if no samples exist in the range.</returns>
	Task<double?> ReadMaxAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all samples of <paramref name="healthParameter"/> in the specified date range, ordered by end date.
	/// </summary>
	/// <param name="unit">The unit to express values in. Use <see cref="Constants.Units"/> constants.</param>
	/// <remarks>Results are capped at 1000 records per query on Android.</remarks>
	/// <returns>A list of <see cref="Sample"/> objects; empty if no data exists in the range.</returns>
	Task<List<Sample>> ReadAllAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until, string unit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the parameter aggregated into time buckets (hourly / daily / weekly / monthly) over the
	/// range — e.g. daily step totals or a weekly average weight — so callers don't have to bucket by
	/// hand. Cumulative parameters are summed per bucket; discrete parameters are averaged.
	/// </summary>
	/// <param name="interval">The bucket size. Buckets align to local-time boundaries.</param>
	/// <param name="unit">The unit for the values. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>One <see cref="StatisticsBucket"/> per interval across the range (including empty buckets).</returns>
	Task<List<StatisticsBucket>> ReadStatisticsAsync(HealthParameter healthParameter, DateTimeOffset from,
		DateTimeOffset until, StatisticsInterval interval, string unit, CancellationToken cancellationToken = default);

	/// <summary>
	/// Writes a single health value for the given parameter.
	/// </summary>
	/// <param name="date">The timestamp to associate with the value; defaults to now if null.</param>
	/// <param name="unit">The unit of <paramref name="valueToStore"/>. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>True if the value was stored successfully.</returns>
	Task<bool> WriteAsync(HealthParameter healthParameter, DateTimeOffset? date, double valueToStore, string unit,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Writes several timestamped values for the same parameter in a single call — far cheaper than a
	/// loop of <see cref="WriteAsync"/> for bulk imports. Maps to one HealthKit <c>SaveObjects</c> /
	/// one Health Connect <c>InsertRecords</c>.
	/// </summary>
	/// <param name="values">The (timestamp, value) pairs to write, all in <paramref name="unit"/>.</param>
	/// <param name="unit">The unit of every value. Use <see cref="Constants.Units"/> constants.</param>
	/// <returns>True if the values were stored successfully (true for an empty set).</returns>
	Task<bool> WriteAllAsync(HealthParameter healthParameter, IEnumerable<(DateTimeOffset date, double value)> values,
		string unit, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes all of this app's samples for the given parameter within the date range.
	/// </summary>
	/// <remarks>
	/// Only data your app wrote can be deleted. On Android, parameters that share a Health Connect record
	/// type (e.g. systolic/diastolic blood pressure, walking/cycling distance, or any nutrient) are stored
	/// together, so deleting one deletes that shared record within the range.
	/// </remarks>
	/// <returns>True if the delete completed (true even when nothing matched).</returns>
	Task<bool> DeleteAsync(HealthParameter healthParameter, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Checks (and requests, on iOS) authorization to read and/or write workout sessions and GPS routes.
	/// Must be called before using <see cref="ReadWorkoutsAsync"/>, <see cref="ReadLatestWorkoutAsync"/>,
	/// or <see cref="WriteWorkoutAsync"/>.
	/// </summary>
	/// <remarks>
	/// On iOS, requests authorization for <c>HKObjectType.WorkoutType</c> and <c>HKSeriesType.WorkoutRouteType</c>
	/// from HealthKit. On Android, checks whether the <c>READ_EXERCISE</c> / <c>WRITE_EXERCISE</c>
	/// Health Connect permissions are already granted — it does NOT launch the consent UI.
	/// </remarks>
	/// <returns>True if the required workout permissions are granted.</returns>
	Task<bool> CheckWorkoutPermissionAsync(PermissionType permissionType, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests authorization to read and/or write workout sessions and GPS routes by presenting the
	/// platform's consent UI, then reports whether the permission ends up granted.
	/// </summary>
	/// <remarks>
	/// On iOS, requests authorization for <c>HKObjectType.WorkoutType</c> and
	/// <c>HKSeriesType.WorkoutRouteType</c>. On Android, if the workout permissions are already granted
	/// this returns <c>true</c> without prompting; otherwise it launches the Health Connect consent UI
	/// for the <c>EXERCISE</c> (and exercise-route) permissions using the current foreground
	/// <c>Activity</c> and re-checks after the user returns.
	/// </remarks>
	/// <returns>True if the required workout permissions are granted after the request completes.</returns>
	Task<bool> RequestWorkoutPermissionAsync(PermissionType permissionType, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns all workout sessions of the specified type recorded in the given date range.
	/// Use <see cref="WorkoutType.Other"/> to return all workout types.
	/// </summary>
	Task<List<WorkoutSession>> ReadWorkoutsAsync(WorkoutType workoutType, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the most recent workout session of the specified type in the given date range.
	/// Use <see cref="WorkoutType.Other"/> to match any workout type.
	/// </summary>
	/// <returns>The latest <see cref="WorkoutSession"/>, or null if none exists.</returns>
	Task<WorkoutSession?> ReadLatestWorkoutAsync(WorkoutType workoutType, DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default);

	/// <summary>Writes a workout session, including its GPS route if provided.</summary>
	/// <returns>True if the session was stored successfully.</returns>
	Task<bool> WriteWorkoutAsync(WorkoutSession session, CancellationToken cancellationToken = default);

	/// <summary>
	/// Requests authorization to read and/or write sleep data by presenting the platform's consent UI,
	/// then reports whether the permission ends up granted.
	/// </summary>
	/// <remarks>
	/// On iOS this authorizes the <c>HKCategoryType.SleepAnalysis</c> type. On Android it requests the
	/// <c>READ_SLEEP</c> / <c>WRITE_SLEEP</c> Health Connect permissions (launching the consent UI when
	/// not already granted, then re-checking).
	/// </remarks>
	/// <returns>True if the required sleep permissions are granted after the request completes.</returns>
	Task<bool> RequestSleepPermissionAsync(PermissionType permissionType, CancellationToken cancellationToken = default);

	/// <summary>
	/// Returns the sleep sessions recorded in the given date range, each with its stage breakdown.
	/// </summary>
	/// <remarks>
	/// On Android each Health Connect <c>SleepSessionRecord</c> maps to one <see cref="SleepSession"/>.
	/// On iOS the individual <c>SleepAnalysis</c> category samples are grouped into sessions by time gaps.
	/// </remarks>
	Task<List<SleepSession>> ReadSleepAsync(DateTimeOffset from, DateTimeOffset until,
		CancellationToken cancellationToken = default);

	/// <summary>Writes a sleep session, including its stage segments.</summary>
	/// <returns>True if the session was stored successfully.</returns>
	Task<bool> WriteSleepAsync(SleepSession session, CancellationToken cancellationToken = default);
}
