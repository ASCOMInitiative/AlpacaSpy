using AlpacaSpy.Models;

namespace AlpacaSpy
{
    public enum SelectiveLogMemberType
    {
        PropertyGet,
        PropertySet,
        Method,
        Function
    }

    public sealed record SelectiveLogMember(string MemberName, SelectiveLogMemberType MemberType)
    {
        public string DisplayName => MemberType switch
        {
            SelectiveLogMemberType.Method => $"{MemberName}()",
            SelectiveLogMemberType.PropertyGet => $"{MemberName} [get]",
            SelectiveLogMemberType.PropertySet => $"{MemberName} [set]",
            _ => MemberName
        };

        public string SelectionKey => $"{MemberType}:{MemberName}";
    }

    public static class SelectiveLoggingMetadata
    {
        private static readonly IReadOnlyList<SelectiveLogMember> CommonMembers =
        [
            new("Action", SelectiveLogMemberType.Method),
            new("CommandBlind", SelectiveLogMemberType.Method),
            new("CommandBool", SelectiveLogMemberType.Method),
            new("CommandString", SelectiveLogMemberType.Method),
            new("Connect",SelectiveLogMemberType.Method),
            new("Connected", SelectiveLogMemberType.PropertyGet),
            new("Connected", SelectiveLogMemberType.PropertySet),
            new("Connecting", SelectiveLogMemberType.PropertyGet),
            new("Description", SelectiveLogMemberType.PropertyGet),
            new("DeviceState", SelectiveLogMemberType.PropertyGet),
            new("Disconnect",SelectiveLogMemberType.Method),
            new("DriverInfo", SelectiveLogMemberType.PropertyGet),
            new("DriverVersion", SelectiveLogMemberType.PropertyGet),
            new("InterfaceVersion", SelectiveLogMemberType.PropertyGet),
            new("Name", SelectiveLogMemberType.PropertyGet),
            new("SupportedActions", SelectiveLogMemberType.PropertyGet)
        ];

        private static readonly IReadOnlyDictionary<AlpacaDeviceType, IReadOnlyList<SelectiveLogMember>> MembersByDevice =
            new Dictionary<AlpacaDeviceType, IReadOnlyList<SelectiveLogMember>>
            {
                [AlpacaDeviceType.Camera] = BuildMembers(
                    new("BayerOffsetX", SelectiveLogMemberType.PropertyGet),
                    new("BayerOffsetY", SelectiveLogMemberType.PropertyGet),
                    new("BinX", SelectiveLogMemberType.PropertyGet),
                    new("BinX", SelectiveLogMemberType.PropertySet),
                    new("BinY", SelectiveLogMemberType.PropertyGet),
                    new("BinY", SelectiveLogMemberType.PropertySet),
                    new("CameraState", SelectiveLogMemberType.PropertyGet),
                    new("CameraXSize", SelectiveLogMemberType.PropertyGet),
                    new("CameraYSize", SelectiveLogMemberType.PropertyGet),
                    new("CanAbortExposure", SelectiveLogMemberType.PropertyGet),
                    new("CanAsymmetricBin", SelectiveLogMemberType.PropertyGet),
                    new("CanFastReadout", SelectiveLogMemberType.PropertyGet),
                    new("CanGetCoolerPower", SelectiveLogMemberType.PropertyGet),
                    new("CanPulseGuide", SelectiveLogMemberType.PropertyGet),
                    new("CanSetCCDTemperature", SelectiveLogMemberType.PropertyGet),
                    new("CanStopExposure", SelectiveLogMemberType.PropertyGet),
                    new("CCDTemperature", SelectiveLogMemberType.PropertyGet),
                    new("CoolerOn", SelectiveLogMemberType.PropertyGet),
                    new("CoolerOn", SelectiveLogMemberType.PropertySet),
                    new("CoolerPower", SelectiveLogMemberType.PropertyGet),
                    new("ElectronsPerADU", SelectiveLogMemberType.PropertyGet),
                    new("ExposureMax", SelectiveLogMemberType.PropertyGet),
                    new("ExposureMin", SelectiveLogMemberType.PropertyGet),
                    new("ExposureResolution", SelectiveLogMemberType.PropertyGet),
                    new("FastReadout", SelectiveLogMemberType.PropertyGet),
                    new("FastReadout", SelectiveLogMemberType.PropertySet),
                    new("FullWellCapacity", SelectiveLogMemberType.PropertyGet),
                    new("Gain", SelectiveLogMemberType.PropertyGet),
                    new("Gain", SelectiveLogMemberType.PropertySet),
                    new("GainMax", SelectiveLogMemberType.PropertyGet),
                    new("GainMin", SelectiveLogMemberType.PropertyGet),
                    new("Gains", SelectiveLogMemberType.PropertyGet),
                    new("HasShutter", SelectiveLogMemberType.PropertyGet),
                    new("HeatSinkTemperature", SelectiveLogMemberType.PropertyGet),
                    new("ImageArray", SelectiveLogMemberType.PropertyGet),
                    new("ImageArrayVariant", SelectiveLogMemberType.PropertyGet),
                    new("ImageReady", SelectiveLogMemberType.PropertyGet),
                    new("IsPulseGuiding", SelectiveLogMemberType.PropertyGet),
                    new("LastExposureDuration", SelectiveLogMemberType.PropertyGet),
                    new("LastExposureStartTime", SelectiveLogMemberType.PropertyGet),
                    new("MaxADU", SelectiveLogMemberType.PropertyGet),
                    new("MaxBinX", SelectiveLogMemberType.PropertyGet),
                    new("MaxBinY", SelectiveLogMemberType.PropertyGet),
                    new("NumX", SelectiveLogMemberType.PropertyGet),
                    new("NumX", SelectiveLogMemberType.PropertySet),
                    new("NumY", SelectiveLogMemberType.PropertyGet),
                    new("NumY", SelectiveLogMemberType.PropertySet),
                    new("Offset", SelectiveLogMemberType.PropertyGet),
                    new("Offset", SelectiveLogMemberType.PropertySet),
                    new("OffsetMax", SelectiveLogMemberType.PropertyGet),
                    new("OffsetMin", SelectiveLogMemberType.PropertyGet),
                    new("Offsets", SelectiveLogMemberType.PropertyGet),
                    new("PercentCompleted", SelectiveLogMemberType.PropertyGet),
                    new("PixelSizeX", SelectiveLogMemberType.PropertyGet),
                    new("PixelSizeY", SelectiveLogMemberType.PropertyGet),
                    new("ReadoutMode", SelectiveLogMemberType.PropertyGet),
                    new("ReadoutMode", SelectiveLogMemberType.PropertySet),
                    new("ReadoutModes", SelectiveLogMemberType.PropertyGet),
                    new("SensorName", SelectiveLogMemberType.PropertyGet),
                    new("SensorType", SelectiveLogMemberType.PropertyGet),
                    new("SetCCDTemperature", SelectiveLogMemberType.PropertyGet),
                    new("SetCCDTemperature", SelectiveLogMemberType.PropertySet),
                    new("StartX", SelectiveLogMemberType.PropertyGet),
                    new("StartX", SelectiveLogMemberType.PropertySet),
                    new("StartY", SelectiveLogMemberType.PropertyGet),
                    new("StartY", SelectiveLogMemberType.PropertySet),
                    new("SubExposureDuration", SelectiveLogMemberType.PropertyGet),
                    new("SubExposureDuration", SelectiveLogMemberType.PropertySet),
                    new("AbortExposure", SelectiveLogMemberType.Method),
                    new("PulseGuide", SelectiveLogMemberType.Method),
                    new("StartExposure", SelectiveLogMemberType.Method),
                    new("StopExposure", SelectiveLogMemberType.Method)),

                [AlpacaDeviceType.CoverCalibrator] = BuildMembers(
                    new("Brightness", SelectiveLogMemberType.PropertyGet),
                    new("CalibratorChanging", SelectiveLogMemberType.PropertyGet),
                    new("CalibratorState", SelectiveLogMemberType.PropertyGet),
                    new("CoverMoving", SelectiveLogMemberType.PropertyGet),
                    new("CoverState", SelectiveLogMemberType.PropertyGet),
                    new("MaxBrightness", SelectiveLogMemberType.PropertyGet),
                    new("CalibratorOff", SelectiveLogMemberType.Method),
                    new("CalibratorOn", SelectiveLogMemberType.Method),
                    new("CloseCover", SelectiveLogMemberType.Method),
                    new("HaltCover", SelectiveLogMemberType.Method),
                    new("OpenCover", SelectiveLogMemberType.Method)),

                [AlpacaDeviceType.Dome] = BuildMembers(
                    new("Altitude", SelectiveLogMemberType.PropertyGet),
                    new("AtHome", SelectiveLogMemberType.PropertyGet),
                    new("AtPark", SelectiveLogMemberType.PropertyGet),
                    new("Azimuth", SelectiveLogMemberType.PropertyGet),
                    new("CanFindHome", SelectiveLogMemberType.PropertyGet),
                    new("CanPark", SelectiveLogMemberType.PropertyGet),
                    new("CanSetAltitude", SelectiveLogMemberType.PropertyGet),
                    new("CanSetAzimuth", SelectiveLogMemberType.PropertyGet),
                    new("CanSetPark", SelectiveLogMemberType.PropertyGet),
                    new("CanSetShutter", SelectiveLogMemberType.PropertyGet),
                    new("CanSlave", SelectiveLogMemberType.PropertyGet),
                    new("CanSyncAzimuth", SelectiveLogMemberType.PropertyGet),
                    new("ShutterStatus", SelectiveLogMemberType.PropertyGet),
                    new("Slaved", SelectiveLogMemberType.PropertyGet),
                    new("Slaved", SelectiveLogMemberType.PropertySet),
                    new("Slewing", SelectiveLogMemberType.PropertyGet),
                    new("AbortSlew", SelectiveLogMemberType.Method),
                    new("CloseShutter", SelectiveLogMemberType.Method),
                    new("FindHome", SelectiveLogMemberType.Method),
                    new("OpenShutter", SelectiveLogMemberType.Method),
                    new("Park", SelectiveLogMemberType.Method),
                    new("SetPark", SelectiveLogMemberType.Method),
                    new("SlewToAltitude", SelectiveLogMemberType.Method),
                    new("SlewToAzimuth", SelectiveLogMemberType.Method),
                    new("SyncToAzimuth", SelectiveLogMemberType.Method)),

                [AlpacaDeviceType.FilterWheel] = BuildMembers(
                    new("FocusOffsets", SelectiveLogMemberType.PropertyGet),
                    new("Names", SelectiveLogMemberType.PropertyGet),
                    new("Position", SelectiveLogMemberType.PropertyGet),
                    new("Position", SelectiveLogMemberType.PropertySet)),

                [AlpacaDeviceType.Focuser] = BuildMembers(
                    new("Absolute", SelectiveLogMemberType.PropertyGet),
                    new("IsMoving", SelectiveLogMemberType.PropertyGet),
                    new("MaxIncrement", SelectiveLogMemberType.PropertyGet),
                    new("MaxStep", SelectiveLogMemberType.PropertyGet),
                    new("Position", SelectiveLogMemberType.PropertyGet),
                    new("StepSize", SelectiveLogMemberType.PropertyGet),
                    new("TempComp", SelectiveLogMemberType.PropertyGet),
                    new("TempComp", SelectiveLogMemberType.PropertySet),
                    new("TempCompAvailable", SelectiveLogMemberType.PropertyGet),
                    new("Temperature", SelectiveLogMemberType.PropertyGet),
                    new("Halt", SelectiveLogMemberType.Method),
                    new("Move", SelectiveLogMemberType.Method)),

                [AlpacaDeviceType.ObservingConditions] = BuildMembers(
                    new("AveragePeriod", SelectiveLogMemberType.PropertyGet),
                    new("AveragePeriod", SelectiveLogMemberType.PropertySet),
                    new("CloudCover", SelectiveLogMemberType.PropertyGet),
                    new("DewPoint", SelectiveLogMemberType.PropertyGet),
                    new("Humidity", SelectiveLogMemberType.PropertyGet),
                    new("Pressure", SelectiveLogMemberType.PropertyGet),
                    new("RainRate", SelectiveLogMemberType.PropertyGet),
                    new("SkyBrightness", SelectiveLogMemberType.PropertyGet),
                    new("SkyQuality", SelectiveLogMemberType.PropertyGet),
                    new("SkyTemperature", SelectiveLogMemberType.PropertyGet),
                    new("StarFWHM", SelectiveLogMemberType.PropertyGet),
                    new("Temperature", SelectiveLogMemberType.PropertyGet),
                    new("WindDirection", SelectiveLogMemberType.PropertyGet),
                    new("WindGust", SelectiveLogMemberType.PropertyGet),
                    new("WindSpeed", SelectiveLogMemberType.PropertyGet),
                    new("Refresh", SelectiveLogMemberType.Method),
                    new("SensorDescription", SelectiveLogMemberType.Method),
                    new("TimeSinceLastUpdate", SelectiveLogMemberType.Method)),

                [AlpacaDeviceType.Rotator] = BuildMembers(
                    new("CanReverse", SelectiveLogMemberType.PropertyGet),
                    new("IsMoving", SelectiveLogMemberType.PropertyGet),
                    new("MechanicalPosition", SelectiveLogMemberType.PropertyGet),
                    new("Position", SelectiveLogMemberType.PropertyGet),
                    new("Reverse", SelectiveLogMemberType.PropertyGet),
                    new("Reverse", SelectiveLogMemberType.PropertySet),
                    new("StepSize", SelectiveLogMemberType.PropertyGet),
                    new("TargetPosition", SelectiveLogMemberType.PropertyGet),
                    new("Halt", SelectiveLogMemberType.Method),
                    new("Move", SelectiveLogMemberType.Method),
                    new("MoveAbsolute", SelectiveLogMemberType.Method),
                    new("MoveMechanical", SelectiveLogMemberType.Method),
                    new("Sync", SelectiveLogMemberType.Method)),

                [AlpacaDeviceType.SafetyMonitor] = BuildMembers(
                    new SelectiveLogMember("IsSafe", SelectiveLogMemberType.PropertyGet)),

                [AlpacaDeviceType.Switch] = BuildMembers(
                    new("MaxSwitch", SelectiveLogMemberType.PropertyGet),
                    new("CanAsync", SelectiveLogMemberType.Method),
                    new("CancelAsync", SelectiveLogMemberType.Method),
                    new("CanWrite", SelectiveLogMemberType.Method),
                    new("GetSwitch", SelectiveLogMemberType.Method),
                    new("GetSwitchDescription", SelectiveLogMemberType.Method),
                    new("GetSwitchName", SelectiveLogMemberType.Method),
                    new("GetSwitchValue", SelectiveLogMemberType.Method),
                    new("MaxSwitchValue", SelectiveLogMemberType.Method),
                    new("MinSwitchValue", SelectiveLogMemberType.Method),
                    new("SetAsync", SelectiveLogMemberType.Method),
                    new("SetAsyncValue", SelectiveLogMemberType.Method),
                    new("SetSwitch", SelectiveLogMemberType.Method),
                    new("SetSwitchName", SelectiveLogMemberType.Method),
                    new("SetSwitchValue", SelectiveLogMemberType.Method),
                    new("StateChangeComplete", SelectiveLogMemberType.Method),
                    new("SwitchStep", SelectiveLogMemberType.Method)),

                [AlpacaDeviceType.Telescope] = BuildMembers(
                    new("AlignmentMode", SelectiveLogMemberType.PropertyGet),
                    new("Altitude", SelectiveLogMemberType.PropertyGet),
                    new("ApertureArea", SelectiveLogMemberType.PropertyGet),
                    new("ApertureDiameter", SelectiveLogMemberType.PropertyGet),
                    new("AtHome", SelectiveLogMemberType.PropertyGet),
                    new("AtPark", SelectiveLogMemberType.PropertyGet),
                    new("Azimuth", SelectiveLogMemberType.PropertyGet),
                    new("CanFindHome", SelectiveLogMemberType.PropertyGet),
                    new("CanPark", SelectiveLogMemberType.PropertyGet),
                    new("CanPulseGuide", SelectiveLogMemberType.PropertyGet),
                    new("CanSetDeclinationRate", SelectiveLogMemberType.PropertyGet),
                    new("CanSetGuideRates", SelectiveLogMemberType.PropertyGet),
                    new("CanSetPark", SelectiveLogMemberType.PropertyGet),
                    new("CanSetPierSide", SelectiveLogMemberType.PropertyGet),
                    new("CanSetRightAscensionRate", SelectiveLogMemberType.PropertyGet),
                    new("CanSetTracking", SelectiveLogMemberType.PropertyGet),
                    new("CanSlew", SelectiveLogMemberType.PropertyGet),
                    new("CanSlewAltAz", SelectiveLogMemberType.PropertyGet),
                    new("CanSlewAltAzAsync", SelectiveLogMemberType.PropertyGet),
                    new("CanSlewAsync", SelectiveLogMemberType.PropertyGet),
                    new("CanSync", SelectiveLogMemberType.PropertyGet),
                    new("CanSyncAltAz", SelectiveLogMemberType.PropertyGet),
                    new("CanUnpark", SelectiveLogMemberType.PropertyGet),
                    new("Declination", SelectiveLogMemberType.PropertyGet),
                    new("DeclinationRate", SelectiveLogMemberType.PropertyGet),
                    new("DeclinationRate", SelectiveLogMemberType.PropertySet),
                    new("DoesRefraction", SelectiveLogMemberType.PropertyGet),
                    new("DoesRefraction", SelectiveLogMemberType.PropertySet),
                    new("EquatorialSystem", SelectiveLogMemberType.PropertyGet),
                    new("FocalLength", SelectiveLogMemberType.PropertyGet),
                    new("GuideRateDeclination", SelectiveLogMemberType.PropertyGet),
                    new("GuideRateDeclination", SelectiveLogMemberType.PropertySet),
                    new("GuideRateRightAscension", SelectiveLogMemberType.PropertyGet),
                    new("GuideRateRightAscension", SelectiveLogMemberType.PropertySet),
                    new("IsPulseGuiding", SelectiveLogMemberType.PropertyGet),
                    new("RightAscension", SelectiveLogMemberType.PropertyGet),
                    new("RightAscensionRate", SelectiveLogMemberType.PropertyGet),
                    new("RightAscensionRate", SelectiveLogMemberType.PropertySet),
                    new("SideOfPier", SelectiveLogMemberType.PropertyGet),
                    new("SideOfPier", SelectiveLogMemberType.PropertySet),
                    new("SiderealTime", SelectiveLogMemberType.PropertyGet),
                    new("SiteElevation", SelectiveLogMemberType.PropertyGet),
                    new("SiteElevation", SelectiveLogMemberType.PropertySet),
                    new("SiteLatitude", SelectiveLogMemberType.PropertyGet),
                    new("SiteLatitude", SelectiveLogMemberType.PropertySet),
                    new("SiteLongitude", SelectiveLogMemberType.PropertyGet),
                    new("SiteLongitude", SelectiveLogMemberType.PropertySet),
                    new("Slewing", SelectiveLogMemberType.PropertyGet),
                    new("SlewSettleTime", SelectiveLogMemberType.PropertyGet),
                    new("SlewSettleTime", SelectiveLogMemberType.PropertySet),
                    new("TargetDeclination", SelectiveLogMemberType.PropertyGet),
                    new("TargetDeclination", SelectiveLogMemberType.PropertySet),
                    new("TargetRightAscension", SelectiveLogMemberType.PropertyGet),
                    new("TargetRightAscension", SelectiveLogMemberType.PropertySet),
                    new("Tracking", SelectiveLogMemberType.PropertyGet),
                    new("Tracking", SelectiveLogMemberType.PropertySet),
                    new("TrackingRate", SelectiveLogMemberType.PropertyGet),
                    new("TrackingRate", SelectiveLogMemberType.PropertySet),
                    new("TrackingRates", SelectiveLogMemberType.PropertyGet),
                    new("UTCDate", SelectiveLogMemberType.PropertyGet),
                    new("UTCDate", SelectiveLogMemberType.PropertySet),
                    new("AbortSlew", SelectiveLogMemberType.Method),
                    new("AxisRates", SelectiveLogMemberType.Function),
                    new("CanMoveAxis", SelectiveLogMemberType.Function),
                    new("DestinationSideOfPier", SelectiveLogMemberType.Function),
                    new("FindHome", SelectiveLogMemberType.Method),
                    new("MoveAxis", SelectiveLogMemberType.Method),
                    new("Park", SelectiveLogMemberType.Method),
                    new("PulseGuide", SelectiveLogMemberType.Method),
                    new("SetPark", SelectiveLogMemberType.Method),
                    new("SlewToAltAz", SelectiveLogMemberType.Method),
                    new("SlewToAltAzAsync", SelectiveLogMemberType.Method),
                    new("SlewToCoordinates", SelectiveLogMemberType.Method),
                    new("SlewToCoordinatesAsync", SelectiveLogMemberType.Method),
                    new("SlewToTarget", SelectiveLogMemberType.Method),
                    new("SlewToTargetAsync", SelectiveLogMemberType.Method),
                    new("SyncToAltAz", SelectiveLogMemberType.Method),
                    new("SyncToCoordinates", SelectiveLogMemberType.Method),
                    new("SyncToTarget", SelectiveLogMemberType.Method),
                    new("Unpark", SelectiveLogMemberType.Method))
            };

        public static IReadOnlyList<SelectiveLogMember> GetMembers(AlpacaDeviceType deviceType)
            => MembersByDevice.TryGetValue(deviceType, out var members) ? members : CommonMembers;

        public static IReadOnlyList<string> GetSelectionKeys(AlpacaDeviceType deviceType)
            => GetMembers(deviceType).Select(member => member.SelectionKey).ToArray();

        public static bool IsMemberEnabled(ConfiguredDevice device, SelectiveLogMember member)
        {
            if (device.EnabledLogMembers is null)
                return true;

            string key = member.SelectionKey;
            if (device.EnabledLogMembers.Any(m => string.Equals(m, key, StringComparison.OrdinalIgnoreCase)))
                return true;

            // Backward compatibility with pre-enum settings that only stored member names.
            return device.EnabledLogMembers.Any(m => string.Equals(m, member.MemberName, StringComparison.OrdinalIgnoreCase));
        }

        public static void SetMemberEnabled(ConfiguredDevice device, SelectiveLogMember member, bool enabled)
        {
            device.EnabledLogMembers ??= GetSelectionKeys(device.DeviceType).ToList();

            if (enabled)
            {
                if (!device.EnabledLogMembers.Any(m => string.Equals(m, member.SelectionKey, StringComparison.OrdinalIgnoreCase)))
                    device.EnabledLogMembers.Add(member.SelectionKey);
                return;
            }

            device.EnabledLogMembers.RemoveAll(m =>
                string.Equals(m, member.SelectionKey, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m, member.MemberName, StringComparison.OrdinalIgnoreCase));
        }

        public static bool NormalizeDeviceSelection(ConfiguredDevice device)
        {
            if (device.EnabledLogMembers is null)
                return false;

            var normalized = new List<string>();
            foreach (string entry in device.EnabledLogMembers)
            {
                if (TryParseSelectionKey(entry, out SelectiveLogMemberType memberType, out string memberName))
                {
                    normalized.Add($"{memberType}:{memberName}");
                    continue;
                }

                normalized.AddRange(
                    GetMembers(device.DeviceType)
                        .Where(member => string.Equals(member.MemberName, entry, StringComparison.OrdinalIgnoreCase))
                        .Select(member => member.SelectionKey));
            }

            normalized = normalized
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (device.EnabledLogMembers.SequenceEqual(normalized, StringComparer.OrdinalIgnoreCase))
                return false;

            device.EnabledLogMembers = normalized;
            return true;
        }

        public static void EnableAllMembers(ConfiguredDevice device)
        {
            device.EnabledLogMembers = GetSelectionKeys(device.DeviceType).ToList();
        }

        public static void DisableAllMembers(ConfiguredDevice device)
        {
            device.EnabledLogMembers = new List<string>();
        }

        public static SelectiveLogMemberType ResolveMemberType(AlpacaDeviceType deviceType, string memberName, string httpMethod)
        {
            var members = GetMembers(deviceType).Where(member => string.Equals(member.MemberName, memberName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (members.Any(member => member.MemberType == SelectiveLogMemberType.Method))
                return SelectiveLogMemberType.Method;

            if (string.Equals(httpMethod, "PUT", StringComparison.OrdinalIgnoreCase) &&
                members.Any(member => member.MemberType == SelectiveLogMemberType.PropertySet))
            {
                return SelectiveLogMemberType.PropertySet;
            }

            if (members.Any(member => member.MemberType == SelectiveLogMemberType.PropertyGet))
                return SelectiveLogMemberType.PropertyGet;

            return members.FirstOrDefault()?.MemberType ?? SelectiveLogMemberType.PropertyGet;
        }

        private static bool TryParseSelectionKey(string selectionKey, out SelectiveLogMemberType memberType, out string memberName)
        {
            int separatorIndex = selectionKey.IndexOf(':');
            if (separatorIndex <= 0 || separatorIndex >= selectionKey.Length - 1)
            {
                memberType = default;
                memberName = string.Empty;
                return false;
            }

            if (!Enum.TryParse(selectionKey[..separatorIndex], true, out memberType))
            {
                memberName = string.Empty;
                return false;
            }

            memberName = selectionKey[(separatorIndex + 1)..];
            return true;
        }

        private static IReadOnlyList<SelectiveLogMember> BuildMembers(params SelectiveLogMember[] deviceMembers)
            => [.. CommonMembers, .. deviceMembers];
    }
}
