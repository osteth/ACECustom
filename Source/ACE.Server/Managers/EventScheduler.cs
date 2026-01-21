using System;
using System.Globalization;
using ACE.Common.Performance;
using ACE.Entity.Enum;
using ACE.Server.WorldObjects;

using log4net;

namespace ACE.Server.Managers
{
    public static class EventScheduler
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static int currentWeek = -1;
        private static int currentMonth = -1;
        private static int currentQuarter = -1;

        /// <summary>
        /// The rate at which EventScheduler.Tick() executes
        /// </summary>
        private static readonly RateLimiter updateEventSchedulerRateLimiter = new RateLimiter(1, TimeSpan.FromMinutes(5));

        /// <summary>
        /// Initializes the event scheduler and sets up initial events
        /// </summary>
        public static void Initialize()
        {
            log.Info("[EVENTSCHEDULER] Initializing event scheduler...");

            // Calculate current periods
            currentWeek = GetCurrentWeek();
            currentMonth = GetCurrentMonth();
            currentQuarter = GetCurrentQuarter();

            // Enable and start appropriate events
            UpdateScheduledEvents();

            log.Info($"[EVENTSCHEDULER] Event scheduler initialized. Current: Week{currentWeek}, Month{currentMonth}, Quarter{currentQuarter}");
        }

        /// <summary>
        /// Called periodically to check for period changes and update events
        /// </summary>
        public static void Tick()
        {
            if (updateEventSchedulerRateLimiter.GetSecondsToWaitBeforeNextEvent() > 0)
                return;

            updateEventSchedulerRateLimiter.RegisterEvent();

            var newWeek = GetCurrentWeek();
            var newMonth = GetCurrentMonth();
            var newQuarter = GetCurrentQuarter();

            // Check if any period has changed
            if (newWeek != currentWeek || newMonth != currentMonth || newQuarter != currentQuarter)
            {
                log.Info($"[EVENTSCHEDULER] Period change detected. Old: Week{currentWeek}, Month{currentMonth}, Quarter{currentQuarter} -> New: Week{newWeek}, Month{newMonth}, Quarter{newQuarter}");

                currentWeek = newWeek;
                currentMonth = newMonth;
                currentQuarter = newQuarter;

                UpdateScheduledEvents();
            }
        }

        /// <summary>
        /// Gets the current week number (1-52, with week 53 treated as week 52)
        /// </summary>
        public static int GetCurrentWeek()
        {
            var now = DateTime.UtcNow;
            var calendar = CultureInfo.CurrentCulture.Calendar;
            var weekRule = CalendarWeekRule.FirstFourDayWeek;
            var firstDayOfWeek = DayOfWeek.Monday;

            var weekNumber = calendar.GetWeekOfYear(now, weekRule, firstDayOfWeek);

            // Handle edge case: week 53 (partial week at end of year) should be treated as week 52
            return Math.Min(weekNumber, 52);
        }

        /// <summary>
        /// Gets the current month number (1-12)
        /// </summary>
        public static int GetCurrentMonth()
        {
            return DateTime.UtcNow.Month;
        }

        /// <summary>
        /// Gets the current quarter number (1-4)
        /// </summary>
        public static int GetCurrentQuarter()
        {
            var month = DateTime.UtcNow.Month;
            return (month - 1) / 3 + 1;
        }

        /// <summary>
        /// Updates scheduled events based on current week/month/quarter
        /// </summary>
        private static void UpdateScheduledEvents()
        {
            // Handle week events
            for (int week = 1; week <= 52; week++)
            {
                var eventName = $"Week{week}";
                if (!EventManager.IsEventAvailable(eventName))
                    continue;

                var status = EventManager.GetEventStatus(eventName);

                if (week == currentWeek)
                {
                    // Enable and start current week event (if not already enabled/started)
                    if (status != GameEventState.On && status != GameEventState.Enabled)
                    {
                        EventManager.EnableEvent(eventName, null, null);
                        EventManager.StartEvent(eventName, null, null);
                        log.Info($"[EVENTSCHEDULER] Enabled and started {eventName}");
                    }
                    else if (status == GameEventState.Enabled)
                    {
                        // Already enabled, just start it
                        EventManager.StartEvent(eventName, null, null);
                        log.Debug($"[EVENTSCHEDULER] Started {eventName}");
                    }
                }
                else
                {
                    // Stop and disable other week events (DisableEvent already stops if running)
                    if (status == GameEventState.On || status == GameEventState.Enabled)
                    {
                        EventManager.DisableEvent(eventName, null, null);
                        log.Info($"[EVENTSCHEDULER] Stopped and disabled {eventName}");
                    }
                }
            }

            // Handle month events
            for (int month = 1; month <= 12; month++)
            {
                var eventName = $"Month{month}";
                if (!EventManager.IsEventAvailable(eventName))
                    continue;

                var status = EventManager.GetEventStatus(eventName);

                if (month == currentMonth)
                {
                    // Enable and start current month event (if not already enabled/started)
                    if (status != GameEventState.On && status != GameEventState.Enabled)
                    {
                        EventManager.EnableEvent(eventName, null, null);
                        EventManager.StartEvent(eventName, null, null);
                        log.Info($"[EVENTSCHEDULER] Enabled and started {eventName}");
                    }
                    else if (status == GameEventState.Enabled)
                    {
                        // Already enabled, just start it
                        EventManager.StartEvent(eventName, null, null);
                        log.Debug($"[EVENTSCHEDULER] Started {eventName}");
                    }
                }
                else
                {
                    // Stop and disable other month events (DisableEvent already stops if running)
                    if (status == GameEventState.On || status == GameEventState.Enabled)
                    {
                        EventManager.DisableEvent(eventName, null, null);
                        log.Info($"[EVENTSCHEDULER] Stopped and disabled {eventName}");
                    }
                }
            }

            // Handle quarter events
            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var eventName = $"Quarter{quarter}";
                if (!EventManager.IsEventAvailable(eventName))
                    continue;

                var status = EventManager.GetEventStatus(eventName);

                if (quarter == currentQuarter)
                {
                    // Enable and start current quarter event (if not already enabled/started)
                    if (status != GameEventState.On && status != GameEventState.Enabled)
                    {
                        EventManager.EnableEvent(eventName, null, null);
                        EventManager.StartEvent(eventName, null, null);
                        log.Info($"[EVENTSCHEDULER] Enabled and started {eventName}");
                    }
                    else if (status == GameEventState.Enabled)
                    {
                        // Already enabled, just start it
                        EventManager.StartEvent(eventName, null, null);
                        log.Debug($"[EVENTSCHEDULER] Started {eventName}");
                    }
                }
                else
                {
                    // Stop and disable other quarter events (DisableEvent already stops if running)
                    if (status == GameEventState.On || status == GameEventState.Enabled)
                    {
                        EventManager.DisableEvent(eventName, null, null);
                        log.Info($"[EVENTSCHEDULER] Stopped and disabled {eventName}");
                    }
                }
            }
        }
    }
}
