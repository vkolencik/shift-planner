using System.Globalization;
using System.Text.Json;
using ShiftPlanner.Contracts;
using ShiftPlanner.Domain;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using ShiftPlanner.Contracts.Services;

namespace ShiftPlanner.Services.Persistence;

public class S3ScheduleRepository : IScheduleRepository
{
    private readonly IAmazonS3 _s3Client;
    private readonly IUserService _userService;
    private readonly StorageOptions _options;

    public S3ScheduleRepository(IAmazonS3 s3Client, IUserService userService, IOptions<StorageOptions> options)
    {
        _s3Client = s3Client;
        _userService = userService;
        _options = options.Value;
    }

    public async Task StoreComputedSchedule(ScheduledMonth month, SuccessfulSchedulingResult result)
    {
        throw new NotImplementedException();
        // Create a GetObject request
        var request = new PutObjectRequest
        {
            BucketName = GetBucketName(),
            Key = GetFilePath(month),
        };

        await _s3Client.PutObjectAsync(request);
    }

    private static string GetFilePath(ScheduledMonth month) => $"{GetMonthPrefix(month)}{new DateTime().Ticks}.json";

    private static string GetMonthPrefix(ScheduledMonth month) => $"{month.Year}/{month.Month:D2}/";

    public async Task<Schedule> LoadSchedule(ScheduledMonth month)
    {
        var listResponse = await _s3Client.ListObjectsV2Async(new ListObjectsV2Request() {BucketName = GetBucketName(), Prefix = GetMonthPrefix(month)});
        var latestSchedule = listResponse.S3Objects
            .Where(o => o.Size > 0) // filter out folders
            .MaxBy(o => o.LastModified); // latest

        var request = new GetObjectRequest
        {
            BucketName = GetBucketName(),
            Key = latestSchedule.Key,
        };

        using var response = await _s3Client.GetObjectAsync(request);

        var jsonString = await new StreamReader(response.ResponseStream).ReadToEndAsync();
        var loadedSchedule = JsonSerializer.Deserialize<StoredSchedule>(jsonString);
        if (loadedSchedule is null || loadedSchedule.Shifts is null )
        {
            throw new InvalidOperationException("Unable to parse stored schedule");
        }

        var users = await _userService.GetAllUsers();

        var schedule = new Schedule(loadedSchedule.Shifts.Select(s => ConvertStoredShift(s, users)).ToArray());

        return schedule;
    }

    ScheduledShift ConvertStoredShift(StoredScheduledShift s, IReadOnlyCollection<User> users)
    {
        return new ScheduledShift(new Day(DateOnly.ParseExact(s.Day, "yyyy-MM-dd")), new Workplace(s.Workplace), users.Single(u => u.UserId == s.UserId));
    }

    private string GetBucketName()
    {
        var prefix = _options.StoragePrefix.Replace("_", "");
        var bucketName = "schedule-storage-720500131615";

        return $"{prefix}{bucketName}";
    }

    private record StoredSchedule(IReadOnlyList<StoredScheduledShift> Shifts);

    private record StoredScheduledShift(string Day, string Workplace, string UserId);
}
