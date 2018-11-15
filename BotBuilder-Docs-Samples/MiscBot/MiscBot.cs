// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace MiscBot
{
    public class MiscBot
    {
        /// <summary>
        /// Represents a bot that processes incoming activities.
        /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
        /// This is a Transient lifetime service.  Transient lifetime services are created
        /// each time they're requested. For each Activity received, a new instance of this
        /// class is created. Objects that are expensive to construct, or have a lifetime
        /// beyond the single turn, should be carefully managed.
        /// For example, the <see cref="MemoryStorage"/> object and associated
        /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
        /// </summary>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
        // Define the IDs for the dialogs in the bot's dialog set.
        private const string MainDialogId = "mainDialog";
        private const string CheckInDialogId = "checkInDialog";
        private const string TableDialogId = "tableDialog";
        private const string AlarmDialogId = "alarmDialog";

        // Define the dialog set for the bot.
        private readonly DialogSet _dialogs;

        // Define the state accessors and the logger for the bot.
        private readonly BotAccessors _accessors;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelBot"/> class.
        /// </summary>
        /// <param name="accessors">Contains the objects to use to manage state.</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> that is hooked to the Azure App Service provider.</param>
        public MiscBot(BotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _logger = loggerFactory.CreateLogger<MiscBot>();
            _logger.LogTrace($"{nameof(MiscBot)} turn start.");
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));

            // Define the steps of the main dialog.
            WaterfallStep[] steps = new WaterfallStep[]
            {
        MenuStepAsync,
        HandleChoiceAsync,
        LoopBackAsync,
            };

            // Create our bot's dialog set, adding a main dialog and the three component dialogs.
            _dialogs = new DialogSet(_accessors.DialogStateAccessor)
                .Add(new WaterfallDialog(MainDialogId, steps))
                .Add(new CheckInDialog(CheckInDialogId))
                .Add(new ReserveTableDialog(TableDialogId))
                .Add(new SetAlarmDialog(AlarmDialogId));
        }

        private static async Task<DialogTurnResult> MenuStepAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Present the user with a set of "suggested actions".
            List<string> menu = new List<string> { "Reserve Table", "Wake Up" };
            await stepContext.Context.SendActivityAsync(
                MessageFactory.SuggestedActions(menu, "How can I help you?"),
                cancellationToken: cancellationToken);
            return Dialog.EndOfTurn;
        }

        private async Task<DialogTurnResult> HandleChoiceAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get the user's info. (Since the type factory is null, this will throw if state does not yet have a value for user info.)
            UserInfo userInfo = await _accessors.UserInfoAccessor.GetAsync(stepContext.Context, null, cancellationToken);

            // Check the user's input and decide which dialog to start.
            // Pass in the guest info when starting either of the child dialogs.
            string choice = (stepContext.Result as string)?.Trim()?.ToLowerInvariant();
            switch (choice)
            {
                case "reserve table":
                    return await stepContext.BeginDialogAsync(TableDialogId, userInfo.Guest, cancellationToken);

                case "wake up":
                    return await stepContext.BeginDialogAsync(AlarmDialogId, userInfo.Guest, cancellationToken);

                default:
                    // If we don't recognize the user's intent, start again from the beginning.
                    await stepContext.Context.SendActivityAsync(
                        "Sorry, I don't understand that command. Please choose an option from the list.");
                    return await stepContext.ReplaceDialogAsync(MainDialogId, null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> LoopBackAsync(
            WaterfallStepContext stepContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Get the user's info. (Because the type factory is null, this will throw if state does not yet have a value for user info.)
            UserInfo userInfo = await _accessors.UserInfoAccessor.GetAsync(stepContext.Context, null, cancellationToken);

            // Process the return value from the child dialog.
            switch (stepContext.Result)
            {
                case TableInfo table:
                    // Store the results of the reserve-table dialog.
                    userInfo.Table = table;
                    await _accessors.UserInfoAccessor.SetAsync(stepContext.Context, userInfo, cancellationToken);
                    break;
                case WakeUpInfo alarm:
                    // Store the results of the set-wake-up-call dialog.
                    userInfo.WakeUp = alarm;
                    await _accessors.UserInfoAccessor.SetAsync(stepContext.Context, userInfo, cancellationToken);
                    break;
                default:
                    // We shouldn't get here, since these are no other branches that get this far.
                    break;
            }

            // Restart the main menu dialog.
            return await stepContext.ReplaceDialogAsync(MainDialogId, null, cancellationToken);
        }
    }
}
