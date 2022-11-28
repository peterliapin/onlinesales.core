// <copyright file="SmsService.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using Microsoft.Extensions.Configuration;
using OnlineSales.Infrastructure;
using OnlineSales.Plugin.Sms.Configuration;
using OnlineSales.Plugin.Sms.Exceptions;
using PhoneNumbers;

namespace OnlineSales.Plugin.Sms.Services;

public class SmsService : ISmsService
{
    private readonly PluginConfig pluginSettings = new PluginConfig();
    private readonly Dictionary<string, ISmsService> countrySmsServices = new Dictionary<string, ISmsService>();
    private readonly PhoneNumberUtil phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public SmsService(IConfiguration configuration)
    {
        var settings = configuration.Get<PluginConfig>();

        if (settings != null)
        {
            pluginSettings = settings;
            InitGateways();
        }
    }

    public Task SendAsync(string recipient, string message)
    {
        var phoneNumber = phoneNumberUtil.Parse(recipient, string.Empty);

        ISmsService? smsService;

        if (countrySmsServices.TryGetValue("+" + phoneNumber.CountryCode, out smsService))
        {
            // nothing here
        }
        else if (countrySmsServices.TryGetValue("default", out smsService))
        {
            // nothing here
        }

        if (smsService == null)
        {
            throw new UnknownCountryCodeException();
        }

        return smsService.SendAsync(recipient, message);
    }

    private void InitGateways()
    {
        foreach (var countryGateway in pluginSettings.CountryGateways)
        {
            ISmsService? gatewayService = null;

            var gatewayName = countryGateway.Gateway;

            switch (gatewayName)
            {
                case "Smsc":
                    gatewayService = new SmscService(pluginSettings.Gateways.Smsc);
                    break;
                case "Getshoutout":
                    gatewayService = new GetshoutoutService(pluginSettings.Gateways.Getshoutout);
                    break;
                case "AmazonSns":
                    gatewayService = new AmazonSnsGatewayService(pluginSettings.Gateways.AmazonSns);
                    break;
            }

            if (gatewayService != null)
            {
                countrySmsServices[countryGateway.Code] = gatewayService;
            }            
        }   
    }
}