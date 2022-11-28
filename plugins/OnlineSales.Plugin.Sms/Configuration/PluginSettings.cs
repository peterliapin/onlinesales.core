// <copyright file="PluginSettings.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

namespace OnlineSales.Plugin.Sms.Configuration;

public class PluginConfig
{
    public string SmsAccessKey { get; set; } = string.Empty;

    public GatewaysConfig Gateways { get; set; } = new GatewaysConfig();

    public List<CountryGatewayConfig> CountryGateways { get; set; } = new List<CountryGatewayConfig>();
}

public class GatewaysConfig
{
    public AmazonSnsConfig AmazonSns { get; set; } = new AmazonSnsConfig();

    public SmscConfig Smsc { get; set; } = new SmscConfig();

    public GetshoutoutConfig Getshoutout { get; set; } = new GetshoutoutConfig();
}

public class CountryGatewayConfig
{
    public string Code { get; set; } = string.Empty;

    public string Gateway { get; set; } = string.Empty;
}

/// <summary>
/// API Documentation: https://docs.aws.amazon.com/sns/latest/api/welcome.html.
/// </summary>
public class AmazonSnsConfig
{
    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;

    public string DefaultRegion { get; set; } = string.Empty;
}

/// <summary>
/// API Documentation: https://developers.getshoutout.com/#liteapimessage_send.
/// </summary>
public class GetshoutoutConfig
{
    public string SenderId { get; set; } = string.Empty;

    public string ApiUrl { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// API Documentation: https://smsc.ru/api/.
/// </summary>
public class SmscConfig
{
    public string SenderId { get; set; } = string.Empty;

    public string ApiUrl { get; set; } = string.Empty;    

    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;    
}
    
