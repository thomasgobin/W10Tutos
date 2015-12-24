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

        private Dictionary<string, string> _series;
        public CortanaVoiceCommandService()
        {
            _series = new Dictionary<string, string>();
            _series.Add("homeland", "Homeland");
            _series.Add("jessica-jones", "Jessica Jones");
            _series.Add("breaking-bad", "Breaking Bad");
            _series.Add("american-dad", "American Dad");
            _series.Add("game-of-thrones", "Game of Thrones");
        }

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

                    await ShowSeries();
                    //await LaunchAppInForeground();
                }
                catch (Exception ex)
                { }
                finally
                {
                    this.serviceDeferral.Complete();
                }
            }
        }

        private async Task ShowSeries()
        {
            //Création d'un message de réponse temporaire permettant de ne plus couper la tâche au bout de XX secondes par défaut
            var userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = "Nous récupérons vos séries";

            VoiceCommandResponse response_temp = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response_temp);

            //Création de la liste de résultats à afficher par Cortana
            var destinationsContentTiles = new List<VoiceCommandContentTile>();

            //Création d'une tuile pour chaque série
            foreach (var serie in _series)
            {
                var tile = new VoiceCommandContentTile();
                tile.ContentTileType = VoiceCommandContentTileType.TitleOnly;
                tile.AppLaunchArgument = "serie-"+serie.Key;
                tile.Title = serie.Value;

                destinationsContentTiles.Add(tile);
            }


            //Message de résultat
            var userReprompt = new VoiceCommandUserMessage();
            userReprompt.DisplayMessage = "Vos séries";
            userReprompt.SpokenMessage = "Voici vos séries";

            var response = VoiceCommandResponse.CreateResponse(userReprompt, destinationsContentTiles);
            await voiceServiceConnection.ReportSuccessAsync(response);
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
