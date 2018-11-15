using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace MiscBot
{
    public class SetAlarmDialog : ComponentDialog
    {
        private const string AlarmPrompt = "dateTimePrompt";

        public SetAlarmDialog(string id)
            : base(id)
        {
            InitialDialogId = Id;

            // Define the prompts used in this conversation flow.
            // Ideally, we'd add validation to this prompt.
            AddDialog(new DateTimePrompt(AlarmPrompt));

            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                AlarmStepAsync,
                FinalStepAsync,
            };

            AddDialog(new WaterfallDialog(Id, waterfallSteps));
        }

        private static async Task<DialogTurnResult> AlarmStepAsync(
            WaterfallStepContext step,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string greeting = step.Options is GuestInfo guest
                    && !string.IsNullOrWhiteSpace(guest?.Name)
                    ? $"Hi {guest.Name}" : "Hi";

            string prompt = $"{greeting}. When would you like your alarm set for?";
            return await step.PromptAsync(
                AlarmPrompt,
                new PromptOptions { Prompt = MessageFactory.Text(prompt) },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> FinalStepAsync(
            WaterfallStepContext step,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Ambiguous responses can generate multiple results.
            var resolution = (step.Result as IList<DateTimeResolution>)?.FirstOrDefault();

            // Time ranges have a start and no value.
            var alarm = resolution.Value ?? resolution.Start;
            string roomNumber = (step.Options as GuestInfo)?.Room;

            // Send a confirmation message.
            await step.Context.SendActivityAsync(
                $"Your alarm is set to {alarm} for room {roomNumber}.",
                cancellationToken: cancellationToken);

            // End the dialog, returning the alarm info.
            return await step.EndDialogAsync(
                new WakeUpInfo { Time = alarm },
                cancellationToken);
        }
    }
}
