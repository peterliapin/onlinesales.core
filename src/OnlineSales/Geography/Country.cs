// <copyright file="Country.cs" company="WavePoint Co. Ltd.">
// Licensed under the MIT license. See LICENSE file in the samples root for full license information.
// </copyright>

using System.ComponentModel;

namespace OnlineSales.Geography;

public enum Country
{
    [Description("Unknown")]
    ZZ = 0,

    [Description("Afghanistan")]
    AF = 4,
    [Description("Åland Islands")]
    AX = 248,
    [Description("Albania")]
    AL = 8,
    [Description("Algeria")]
    DZ = 12,
    [Description("American Samoa")]
    AS = 16,
    [Description("Andorra")]
    AD = 20,
    [Description("Angola")]
    AO = 24,
    [Description("Anguilla")]
    AI = 660,
    [Description("Antarctica")]
    AQ = 10,
    [Description("Antigua and Barbuda")]
    AG = 28,
    [Description("Argentina")]
    AR = 32,
    [Description("Armenia")]
    AM = 51,
    [Description("Aruba")]
    AW = 533,
    [Description("Australia")]
    AU = 36,
    [Description("Austria")]
    AT = 40,
    [Description("Azerbaijan")]
    AZ = 31,
    [Description("Bahamas (the)")]
    BS = 44,
    [Description("Bahrain")]
    BH = 48,
    [Description("Bangladesh")]
    BD = 50,
    [Description("Barbados")]
    BB = 52,
    [Description("Belarus")]
    BY = 112,
    [Description("Belgium")]
    BE = 56,
    [Description("Belize")]
    BZ = 84,
    [Description("Benin")]
    BJ = 204,
    [Description("Bermuda")]
    BM = 60,
    [Description("Bhutan")]
    BT = 64,
    [Description("Bolivia (Plurinational State of)")]
    BO = 68,
    [Description("Bonaire, Sint Eustatius and Saba")]
    BQ = 535,
    [Description("Bosnia and Herzegovina")]
    BA = 70,
    [Description("Botswana")]
    BW = 72,
    [Description("Bouvet Island")]
    BV = 74,
    [Description("Brazil")]
    BR = 76,
    [Description("British Indian Ocean Territory (the)")]
    IO = 86,
    [Description("Brunei Darussalam")]
    BN = 96,
    [Description("Bulgaria")]
    BG = 100,
    [Description("Burkina Faso")]
    BF = 854,
    [Description("Burundi")]
    BI = 108,
    [Description("Cabo Verde")]
    CV = 132,
    [Description("Cambodia")]
    KH = 116,
    [Description("Cameroon")]
    CM = 120,
    [Description("Canada")]
    CA = 124,
    [Description("Cayman Islands (the)")]
    KY = 136,
    [Description("Central African Republic (the)")]
    CF = 140,
    [Description("Chad")]
    TD = 148,
    [Description("Chile")]
    CL = 152,
    [Description("China")]
    CN = 156,
    [Description("Christmas Island")]
    CX = 162,
    [Description("Cocos (Keeling) Islands (the)")]
    CC = 166,
    [Description("Colombia")]
    CO = 170,
    [Description("Comoros (the)")]
    KM = 174,
    [Description("Congo (the Democratic Republic of the)")]
    CD = 180,
    [Description("Congo (the)")]
    CG = 178,
    [Description("Cook Islands (the)")]
    CK = 184,
    [Description("Costa Rica")]
    CR = 188,
    [Description("Côte d'Ivoire")]
    CI = 384,
    [Description("Croatia")]
    HR = 191,
    [Description("Cuba")]
    CU = 192,
    [Description("Curaçao")]
    CW = 531,
    [Description("Cyprus")]
    CY = 196,
    [Description("Czechia")]
    CZ = 203,
    [Description("Denmark")]
    DK = 208,
    [Description("Djibouti")]
    DJ = 262,
    [Description("Dominica")]
    DM = 212,
    [Description("Dominican Republic (the)")]
    DO = 214,
    [Description("Ecuador")]
    EC = 218,
    [Description("Egypt")]
    EG = 818,
    [Description("El Salvador")]
    SV = 222,
    [Description("Equatorial Guinea")]
    GQ = 226,
    [Description("Eritrea")]
    ER = 232,
    [Description("Estonia")]
    EE = 233,
    [Description("Eswatini")]
    SZ = 748,
    [Description("Ethiopia")]
    ET = 231,
    [Description("Falkland Islands (the) [Malvinas]")]
    FK = 238,
    [Description("Faroe Islands (the)")]
    FO = 234,
    [Description("Fiji")]
    FJ = 242,
    [Description("Finland")]
    FI = 246,
    [Description("France")]
    FR = 250,
    [Description("French Guiana")]
    GF = 254,
    [Description("French Polynesia")]
    PF = 258,
    [Description("French Southern Territories (the)")]
    TF = 260,
    [Description("Gabon")]
    GA = 266,
    [Description("Gambia (the)")]
    GM = 270,
    [Description("Georgia")]
    GE = 268,
    [Description("Germany")]
    DE = 276,
    [Description("Ghana")]
    GH = 288,
    [Description("Gibraltar")]
    GI = 292,
    [Description("Greece")]
    GR = 300,
    [Description("Greenland")]
    GL = 304,
    [Description("Grenada")]
    GD = 308,
    [Description("Guadeloupe")]
    GP = 312,
    [Description("Guam")]
    GU = 316,
    [Description("Guatemala")]
    GT = 320,
    [Description("Guernsey")]
    GG = 831,
    [Description("Guinea")]
    GN = 324,
    [Description("Guinea-Bissau")]
    GW = 624,
    [Description("Guyana")]
    GY = 328,
    [Description("Haiti")]
    HT = 332,
    [Description("Heard Island and McDonald Islands")]
    HM = 334,
    [Description("Holy See (the)")]
    VA = 336,
    [Description("Honduras")]
    HN = 340,
    [Description("Hong Kong")]
    HK = 344,
    [Description("Hungary")]
    HU = 348,
    [Description("Iceland")]
    IS = 352,
    [Description("India")]
    IN = 356,
    [Description("Indonesia")]
    ID = 360,
    [Description("Iran (Islamic Republic of)")]
    IR = 364,
    [Description("Iraq")]
    IQ = 368,
    [Description("Ireland")]
    IE = 372,
    [Description("Isle of Man")]
    IM = 833,
    [Description("Israel")]
    IL = 376,
    [Description("Italy")]
    IT = 380,
    [Description("Jamaica")]
    JM = 388,
    [Description("Japan")]
    JP = 392,
    [Description("Jersey")]
    JE = 832,
    [Description("Jordan")]
    JO = 400,
    [Description("Kazakhstan")]
    KZ = 398,
    [Description("Kenya")]
    KE = 404,
    [Description("Kiribati")]
    KI = 296,
    [Description("Korea (the Democratic People's Republic of)")]
    KP = 408,
    [Description("Korea (the Republic of)")]
    KR = 410,
    [Description("Kuwait")]
    KW = 414,
    [Description("Kyrgyzstan")]
    KG = 417,
    [Description("Lao People's Democratic Republic (the)")]
    LA = 418,
    [Description("Latvia")]
    LV = 428,
    [Description("Lebanon")]
    LB = 422,
    [Description("Lesotho")]
    LS = 426,
    [Description("Liberia")]
    LR = 430,
    [Description("Libya")]
    LY = 434,
    [Description("Liechtenstein")]
    LI = 438,
    [Description("Lithuania")]
    LT = 440,
    [Description("Luxembourg")]
    LU = 442,
    [Description("Macao")]
    MO = 446,
    [Description("Madagascar")]
    MG = 450,
    [Description("Malawi")]
    MW = 454,
    [Description("Malaysia")]
    MY = 458,
    [Description("Maldives")]
    MV = 462,
    [Description("Mali")]
    ML = 466,
    [Description("Malta")]
    MT = 470,
    [Description("Marshall Islands (the)")]
    MH = 584,
    [Description("Martinique")]
    MQ = 474,
    [Description("Mauritania")]
    MR = 478,
    [Description("Mauritius")]
    MU = 480,
    [Description("Mayotte")]
    YT = 175,
    [Description("Mexico")]
    MX = 484,
    [Description("Micronesia (Federated States of)")]
    FM = 583,
    [Description("Moldova (the Republic of)")]
    MD = 498,
    [Description("Monaco")]
    MC = 492,
    [Description("Mongolia")]
    MN = 496,
    [Description("Montenegro")]
    ME = 499,
    [Description("Montserrat")]
    MS = 500,
    [Description("Morocco")]
    MA = 504,
    [Description("Mozambique")]
    MZ = 508,
    [Description("Myanmar")]
    MM = 104,
    [Description("Namibia")]
    NA = 516,
    [Description("Nauru")]
    NR = 520,
    [Description("Nepal")]
    NP = 524,
    [Description("Netherlands (the)")]
    NL = 528,
    [Description("New Caledonia")]
    NC = 540,
    [Description("New Zealand")]
    NZ = 554,
    [Description("Nicaragua")]
    NI = 558,
    [Description("Niger (the)")]
    NE = 562,
    [Description("Nigeria")]
    NG = 566,
    [Description("Niue")]
    NU = 570,
    [Description("Norfolk Island")]
    NF = 574,
    [Description("North Macedonia")]
    MK = 807,
    [Description("Northern Mariana Islands (the)")]
    MP = 580,
    [Description("Norway")]
    NO = 578,
    [Description("Oman")]
    OM = 512,
    [Description("Pakistan")]
    PK = 586,
    [Description("Palau")]
    PW = 585,
    [Description("Palestine, State of")]
    PS = 275,
    [Description("Panama")]
    PA = 591,
    [Description("Papua New Guinea")]
    PG = 598,
    [Description("Paraguay")]
    PY = 600,
    [Description("Peru")]
    PE = 604,
    [Description("Philippines (the)")]
    PH = 608,
    [Description("Pitcairn")]
    PN = 612,
    [Description("Poland")]
    PL = 616,
    [Description("Portugal")]
    PT = 620,
    [Description("Puerto Rico")]
    PR = 630,
    [Description("Qatar")]
    QA = 634,
    [Description("Réunion")]
    RE = 638,
    [Description("Romania")]
    RO = 642,
    [Description("Russian Federation (the)")]
    RU = 643,
    [Description("Rwanda")]
    RW = 646,
    [Description("Saint Barthélemy")]
    BL = 652,
    [Description("Saint Helena, Ascension and Tristan da Cunha")]
    SH = 654,
    [Description("Saint Kitts and Nevis")]
    KN = 659,
    [Description("Saint Lucia")]
    LC = 662,
    [Description("Saint Martin (French part)")]
    MF = 663,
    [Description("Saint Pierre and Miquelon")]
    PM = 666,
    [Description("Saint Vincent and the Grenadines")]
    VC = 670,
    [Description("Samoa")]
    WS = 882,
    [Description("San Marino")]
    SM = 674,
    [Description("Sao Tome and Principe")]
    ST = 678,
    [Description("Saudi Arabia")]
    SA = 682,
    [Description("Senegal")]
    SN = 686,
    [Description("Serbia")]
    RS = 688,
    [Description("Seychelles")]
    SC = 690,
    [Description("Sierra Leone")]
    SL = 694,
    [Description("Singapore")]
    SG = 702,
    [Description("Sint Maarten (Dutch part)")]
    SX = 534,
    [Description("Slovakia")]
    SK = 703,
    [Description("Slovenia")]
    SI = 705,
    [Description("Solomon Islands")]
    SB = 90,
    [Description("Somalia")]
    SO = 706,
    [Description("South Africa")]
    ZA = 710,
    [Description("South Georgia and the South Sandwich Islands")]
    GS = 239,
    [Description("South Sudan")]
    SS = 728,
    [Description("Spain")]
    ES = 724,
    [Description("Sri Lanka")]
    LK = 144,
    [Description("Sudan (the)")]
    SD = 729,
    [Description("Suriname")]
    SR = 740,
    [Description("Svalbard and Jan Mayen")]
    SJ = 744,
    [Description("Sweden")]
    SE = 752,
    [Description("Switzerland")]
    CH = 756,
    [Description("Syrian Arab Republic (the)")]
    SY = 760,
    [Description("Taiwan (Province of China)")]
    TW = 158,
    [Description("Tajikistan")]
    TJ = 762,
    [Description("Tanzania, the United Republic of")]
    TZ = 834,
    [Description("Thailand")]
    TH = 764,
    [Description("Timor-Leste")]
    TL = 626,
    [Description("Togo")]
    TG = 768,
    [Description("Tokelau")]
    TK = 772,
    [Description("Tonga")]
    TO = 776,
    [Description("Trinidad and Tobago")]
    TT = 780,
    [Description("Tunisia")]
    TN = 788,
    [Description("Türkiye")]
    TR = 792,
    [Description("Turkmenistan")]
    TM = 795,
    [Description("Turks and Caicos Islands (the)")]
    TC = 796,
    [Description("Tuvalu")]
    TV = 798,
    [Description("Uganda")]
    UG = 800,
    [Description("Ukraine")]
    UA = 804,
    [Description("United Arab Emirates (the)")]
    AE = 784,
    [Description("United Kingdom of Great Britain and Northern Ireland (the)")]
    GB = 826,
    [Description("United States Minor Outlying Islands (the)")]
    UM = 581,
    [Description("United States of America (the)")]
    US = 840,
    [Description("Uruguay")]
    UY = 858,
    [Description("Uzbekistan")]
    UZ = 860,
    [Description("Vanuatu")]
    VU = 548,
    [Description("Venezuela (Bolivarian Republic of)")]
    VE = 862,
    [Description("Viet Nam")]
    VN = 704,
    [Description("Virgin Islands (British)")]
    VG = 92,
    [Description("Virgin Islands (U.S.)")]
    VI = 850,
    [Description("Wallis and Futuna")]
    WF = 876,
    [Description("Western Sahara*")]
    EH = 732,
    [Description("Yemen")]
    YE = 887,
    [Description("Zambia")]
    ZM = 894,
    [Description("Zimbabwe")]
    ZW = 716,
}