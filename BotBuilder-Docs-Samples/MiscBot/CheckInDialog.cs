using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace MiscBot
{


    public class CheckInDialog : ComponentDialog
    {
        private const string GuestKey = nameof(CheckInDialog);
        private const string TextPrompt = "textPrompt";

        // You can start this from the parent using the dialog's ID.
        public CheckInDialog(string id)
            : base(id)
        {
            InitialDialogId = Id;

            // Define the prompts used in this conversation flow.
            AddDialog(new TextPrompt(TextPrompt));

            // Define the conversation flow using a waterfall model.
            WaterfallStep[] waterfallSteps = new WaterfallStep[]
            {
            NameStepAsync,
            RoomStepAsync,
            FinalStepAsync,
            };
            AddDialog(new WaterfallDialog(Id, waterfallSteps));
        }

        private static async Task<DialogTurnResult> NameStepAsync(
            WaterfallStepContext step,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Clear the guest information and prompt for the guest's name.
            step.Values[GuestKey] = new GuestInfo();
            return await step.PromptAsync(
                TextPrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("What is your name?"),
                },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> RoomStepAsync(
            WaterfallStepContext step,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Save the name and prompt for the room number.
            string name = step.Result as string;
            ((GuestInfo)step.Values[GuestKey]).Name = name;
            return await step.PromptAsync(
                TextPrompt,
                new PromptOptions
                {
                    Prompt = MessageFactory.Text($"Hi {name}. What room will you be staying in?"),
                },
                cancellationToken);
        }

        private static async Task<DialogTurnResult> FinalStepAsync(
            WaterfallStepContext step,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Save the room number and "sign off".
            string room = step.Result as string;
            ((GuestInfo)step.Values[GuestKey]).Room = room;

            await step.Context.SendActivityAsync(
                "Great, enjoy your stay!",
                cancellationToken: cancellationToken);

            // End the dialog, returning the guest info.
            return await step.EndDialogAsync(
                (GuestInfo)step.Values[GuestKey],
                cancellationToken);
        }
    }
}
