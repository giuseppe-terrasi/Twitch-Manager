using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;
using TwitchManager.Exceptions;
using Microsoft.Extensions.Localization;
using TwitchManager.Translations;

namespace TwitchManager.Components.Abstractions
{
    public class BaseComponent : LoadingComponent
    {
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public ILoggerFactory LoggerFactory { get; set; }

        [Inject]
        public IStringLocalizer<Translation> Localizer { get; set; }

        protected ILogger Logger { get; private set; }

        private protected sealed override RenderFragment Body => BuildRenderTree;

        // Allow content to be provided by a .razor file but without 
        // overriding the content of the base class
        protected new virtual void BuildRenderTree(RenderTreeBuilder builder)
        {
        }

        protected override void OnInitialized()
        {
            Logger = LoggerFactory.CreateLogger(GetType());
        }

        protected async Task DefaultErrorPopupAsync()
        {
            await ShowErrorPopupAsync(Localizer["GenericErrorMessage"]);
        }

        protected async Task ExecuteAsync(Func<Task> task, Func<Task> onSuccess = null, Func<Exception, Task> onError = null)
        {
            await SetLoadingAsync(true);

            try
            {
                await task();

                if (onSuccess != null)
                {
                    await onSuccess();
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is NotFoundException)
            {
                Logger.LogError(ex, "An error occurred");
                NavigationManager.NavigateTo("/not-found");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred");

                if (onError != null)
                {
                    await onError(ex);
                }
                else
                {
                    await DefaultErrorPopupAsync();
                }
            }
            finally
            {
                await SetLoadingAsync(false);
            }
        }
    }
}
