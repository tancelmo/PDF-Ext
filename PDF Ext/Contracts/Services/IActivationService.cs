﻿namespace PDF_Ext.Contracts.Services;

public interface IActivationService
{
    Task ActivateAsync(object activationArgs);
}