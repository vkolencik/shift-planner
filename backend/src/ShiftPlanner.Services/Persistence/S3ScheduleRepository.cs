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
        
        var listResponse = await _s3Client.ListObjectsAsync(new ListObjectsRequest(){BucketName = GetBucketName(), Prefix = GetMonthPrefix(month)});
        var latestSchedule = listResponse.S3Objects.OrderBy(o => o.LastModified).First();
        
        var request = new GetObjectRequest
        {
            BucketName = GetBucketName(),
            Key = latestSchedule.Key,
        };

        using var response = await _s3Client.GetObjectAsync(request);

        var jsonString = await new StreamReader(response.ResponseStream).ReadToEndAsync();
        var loadedSchedule = JsonSerializer.Deserialize<ScheduleWithUserIds>(jsonString);
        if (loadedSchedule is null)
        {
            throw new InvalidOperationException("Unable to parse stored schedule");
        }
        
        var users = await _userService.GetAllUsers();
        var schedule = new Schedule(loadedSchedule.Shifts.Select(s => new ScheduledShift(s.Day, s.Workplace, users.Single(u => u.UserId == s.UserId))).ToArray());

        return schedule;
    }

    private string GetBucketName()
    {
        var prefix = _options.StoragePrefix.Replace("_", "");
        var bucketName = "schedule-storage-720500131615";

        if (prefix != "")
        {
            return $"{prefix}-{bucketName}";
        }

        return bucketName;
    }

    private record ScheduleWithUserIds(IReadOnlyList<ScheduledShiftWithUserId> Shifts);
    private record ScheduledShiftWithUserId(Day Day, Workplace Workplace, string UserId);

}

