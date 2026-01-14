using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace dawn.ftp.ui.ViewModels;

/// <summary>
/// Represents the view model for a password entry functionality, designed to handle
/// user password input operations, including validation, visibility toggling, and
/// dialog interaction management.
/// </summary>
public class PasswordEntryViewModel : ViewModelBase {
    /// <summary>
    /// Represents the view model for password entry functionality.
    /// Provides mechanisms for capturing and validating user-supplied passwords,
    /// handling the visibility state of the password input, and managing interaction
    /// closure events such as confirming or canceling the dialog.
    /// </summary>
    public PasswordEntryViewModel() {
        PasswordReason = string.Empty;
    }

    /// <summary>
    /// Represents the view model for password entry functionality.
    /// Manages the state and behavior associated with password input, including retrieval of the password,
    /// displaying password visibility options, and handling dialog interaction events.
    /// </summary>
    public PasswordEntryViewModel(string passwordReason) {
        PasswordReason = passwordReason;
    }

    private string _password = string.Empty;
    private bool _showPassword;

    /// <summary>
    /// Describes the reason why the user is required to re-enter their password.
    /// This property provides context for the password entry prompt, helping users
    /// understand why their credentials need to be re-entered.
    /// </summary>
    [DefaultValue("We ran into an issue that requires you to re-enter your password for this connection.")]
    public string PasswordReason { get; }

    /// <summary>
    /// Represents the password entered by the user.
    /// The value is updated in response to user input through a password entry field.
    /// This property supports two-way data binding for UI elements.
    /// </summary>
    [DefaultValue("")]
    [Required(ErrorMessage = "This field is required")]
    public string Password {
        get => _password; 
        set => SetProperty(ref _password, value);
    }

    /// <summary>
    /// Determines whether the password entry field should display the password as plain text.
    /// When set to true, the password is revealed; otherwise, it is obscured.
    /// </summary>
    [DefaultValue(false)]
    public bool ShowPassword { 
        get => _showPassword; 
        set => SetProperty(ref _showPassword, value);
    }

    /// <summary>
    /// Helps us determine if the exit was a cancel or an ok.
    /// </summary>
    internal bool WasSuccessfulExit { get; set; }

    /// <summary>
    /// Handles the 'Cancel' button press event, indicating the user has opted to abort the current operation.
    /// Sets the <see cref="WasSuccessfulExit"/> property to false and initiates the dialog closure process.
    /// </summary>
    public void CancelPressed() {
        WasSuccessfulExit = false;
        ExecuteClose(false);
    }

    /// <summary>
    /// Handles the 'Ok' button press event, signifying a successful user interaction.
    /// Sets the <see cref="WasSuccessfulExit"/> property to true and triggers the dialog closure process.
    /// </summary>
    public void OkPressed() {
        WasSuccessfulExit = true;
        ExecuteClose(true);
    }
}