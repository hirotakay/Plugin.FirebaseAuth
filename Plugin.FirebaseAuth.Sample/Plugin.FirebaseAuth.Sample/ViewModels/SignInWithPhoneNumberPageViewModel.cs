﻿using System;
using Reactive.Bindings;
using System.Reactive.Linq;
using Prism.Navigation;
using Plugin.FirebaseAuth.Sample.Extensins;
using Prism.Services;
using Xamarin.Forms;
using System.Threading.Tasks;

namespace Plugin.FirebaseAuth.Sample.ViewModels
{
    public class SignInWithPhoneNumberPageViewModel : ViewModelBase<IMultiFactorSession, IPhoneAuthCredential>
    {
        public ReactivePropertySlim<string> PhoneNumber { get; } = new ReactivePropertySlim<string>();
        public AsyncReactiveCommand SignInCommand { get; }

        private readonly IPageDialogService _pageDialogService;
        private IMultiFactorSession _multiFactorSession;

        public SignInWithPhoneNumberPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService) : base(navigationService)
        {
            Title = "Phone Numbe";

            _pageDialogService = pageDialogService;

            SignInCommand = PhoneNumber.Select(s => !string.IsNullOrEmpty(s)).ToAsyncReactiveCommand();

            SignInCommand.Subscribe(async () =>
            {
                try
                {
                    PhoneNumberVerificationResult verificationResult;

                    if (_multiFactorSession != null)
                    {
                        verificationResult = await CrossFirebaseAuth.Current.PhoneAuthProvider
                            .VerifyPhoneNumberAsync(PhoneNumber.Value, _multiFactorSession);
                    }
                    else
                    {
                        verificationResult = await CrossFirebaseAuth.Current.PhoneAuthProvider
                            .VerifyPhoneNumberAsync(PhoneNumber.Value);
                    }

                    if (verificationResult.Credential != null)
                    {
                        await NavigationService.GoBackAsync(verificationResult.Credential);
                    }
                    else
                    {
                        var verificationCode = await NavigationService.NavigateAsync<VerificationCodePageViewModel, string>();

                        if (verificationCode != null)
                        {
                            var credential = CrossFirebaseAuth.Current.PhoneAuthProvider
                                .GetCredential(verificationResult.VerificationId, verificationCode);

                            await Task.Delay(TimeSpan.FromMilliseconds(1));

                            await NavigationService.GoBackAsync(credential);
                        }
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);

                    await _pageDialogService.DisplayAlertAsync("Failure", e.Message, "OK");
                }
            });
        }

        public override void Prepare(IMultiFactorSession parameer)
        {
            _multiFactorSession = parameer;
        }
    }
}
