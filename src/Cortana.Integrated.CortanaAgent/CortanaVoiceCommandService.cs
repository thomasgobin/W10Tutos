using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace Cortana.Integrated.CortanaAgent
{
    public sealed class CortanaVoiceCommandService : IBackgroundTask
    {
        private BackgroundTaskDeferral serviceDeferral;
        VoiceCommandServiceConnection voiceServiceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            this.serviceDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += (sender, reason) => serviceDeferral?.Complete();

            var triggerDetails =
              taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null &&
              triggerDetails.Name == "CortanaVoiceIntegration")
            {
                try
                {
                    voiceServiceConnection =
                      VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                    voiceServiceConnection.VoiceCommandCompleted += (sender, reason) => serviceDeferral?.Complete();

                    VoiceCommand voiceCommand = await voiceServiceConnection.GetVoiceCommandAsync();

                    await LaunchAppInForeground();
                }
                catch (Exception ex)
                { }
                finally
                {
                    this.serviceDeferral.Complete();
                }
            }
        }

        private async Task LaunchAppInForeground()
        {
            var userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = "Lancement de l'application";

            var response = VoiceCommandResponse.CreateResponse(userMessage);

            response.AppLaunchArgument = "LaunchSeries";

            await voiceServiceConnection.RequestAppLaunchAsync(response);
        }

    }
}
