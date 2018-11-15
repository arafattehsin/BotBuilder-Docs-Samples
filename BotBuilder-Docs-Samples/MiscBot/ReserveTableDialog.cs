using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;

namespace MiscBot
{
    public class ReserveTableDialog : ComponentDialog
    {
        private const string TablePrompt = "choicePrompt";

        public ReserveTableDialog(string id)
            : base(id)
        {
            InitialDialogId = Id;

            // Define the prompts used in this conversation flow.
            AddDialog(new ChoicePrompt(TablePrompt));

            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
                TableStepAsync,
                FinalStepAsync,
            };
            AddDialog(new WaterfallDialog(Id, waterfallSteps));
        }

        private static async Task<DialogTurnResult> TableStepAsync(
            WaterfallStepContext step,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            string greeting = step.Options is GuestInfo guest
                    && !string.IsNullOrWhiteSpace(guest?.Name)
                    ? $"Welcome {guest.Name}" : "Welcome";

            string prompt = $"{greeting}, How many diners will be at your table?";
            string[] choices = new string[] { "1", "2", "3", "4", "5", "6" };
            return await step.PromptAsync(
                TablePrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(prompt),
                    Choices = ChoiceFactory.ToChoices(choices),
                },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> FinalStepAsync(
            WaterfallStepContext step,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // ChoicePrompt returns a FoundChoice object.
            string table = (step.Result as FoundChoice).Value;

            // Send a confirmation message.
            await step.Context.SendActivityAsync(
                $"Sounds great;  we will reserve a table for you for {table} diners.",
                cancellationToken: cancellationToken);

            // End the dialog, returning the table info.
            return await step.EndDialogAsync(
                new TableInfo { Number = table },
                cancellationToken);
        }
    }
}
