// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace MiscBot
{
    /// <summary>
    /// User state information.
    /// </summary>
    public class UserInfo
    {
        public GuestInfo Guest { get; set; }
        public TableInfo Table { get; set; }
        public WakeUpInfo WakeUp { get; set; }
    }

    /// <summary>
    /// State information associated with the check-in dialog.
    /// </summary>
    public class GuestInfo
    {
        public string Name { get; set; }
        public string Room { get; set; }
    }

    /// <summary>
    /// State information associated with the reserve-table dialog.
    /// </summary>
    public class TableInfo
    {
        public string Number { get; set; }
    }

    /// <summary>
    /// State information associated with the wake-up call dialog.
    /// </summary>
    public class WakeUpInfo
    {
        public string Time { get; set; }
    }
}
