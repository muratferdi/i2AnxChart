namespace i2MChart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    /// Created by Murat Ferdi GÜNEY (2022)
    /// </summary>
    public class AnxChart
    {
        public Chart MyChart { get; set; }
        public AnxChart()
        {
            if (MyChart == null)
            {
                ClearChart();
            }
        }


        public void ClearChart()
        {
            MyChart = new Chart();

            #region Chart_Tanımları            
            MyChart.SchemaVersion = "7.0.0.1";
            MyChart.GridHeightSize = 0.2D;
            MyChart.GridWidthSize = 0.2D;
            MyChart.UseDefaultLinkSpacingWhenDragging = false;
            MyChart.GridVisibleOnAllViews = true;
            MyChart.ShowAllFlag = true;
            MyChart.ShowPages = false;
            MyChart.BackColour = 16777215;
            MyChart.DefaultLinkSpacing = 0.2D;
            MyChart.WiringDistanceNear = 0.2D;
            MyChart.UseWiringHeightForThemeIcon = true;
            MyChart.DefaultTickRate = 0.2D;
            MyChart.WiringHeight = 0.2D;
            MyChart.HiddenItemsVisibility = HiddenItemsVisibilityEnum.ItemsVisibilityHidden;
            MyChart.HideMatchingTimeZoneFormat = false;
            MyChart.TimeBarVisible = false;
            MyChart.WiringSpacing = 0.2D;
            MyChart.BlankLinkLabels = false;
            MyChart.UseLocalTimeZone = true;
            MyChart.TypeIconDrawingMode = TypeIconDrawingModeEnum.HighQuality;
            MyChart.WiringDistanceFar = 0.2D;
            MyChart.IdReferenceLinking = false;
            MyChart.IsBackColourFilled = true;
            MyChart.SnapToGrid = true;
            MyChart.LabelSumNumericLinks = false;
            MyChart.LabelRule = LabelMergeAndPasteRuleEnum.LabelRuleMerge;
            MyChart.Rigorous = true;
            MyChart.MsxmlVersion = "4.20.9818.0";
            #endregion

            MyChart.ChartItemCollection = new List<ChartItem>();
            MyChart.StrengthCollection = new List<Strength>();
            MyChart.EntityTypeCollection = new List<EntityType>();
            MyChart.StrengthCollection.Add(new Strength() { Id = "Solid", Name = "Solid", DotStyle = DotStyleEnum.DotStyleSolid });
        } 

        public static string GenerateTextSHA1(string metin)
        {
	        return BitConverter.ToString(System.Security.Cryptography.SHA1.Create().ComputeHash(UTF8Encoding.UTF8.GetBytes(metin))).Replace("-", string.Empty);
        }
        
        public string FindNodeMChart(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = GenerateTextSHA1(id);
                var idList = MyChart.ChartItemCollection.Where(x => x.Item is End).Select(x => x.Item as End).Where(x => x.Item is Entity).Select(x => x.Item as Entity).FirstOrDefault(x => x.EntityId == id);
                if (idList != null)
                {
                    return id;
                }
                else
                {
                    return null;
                }
            }
            return id;
        }

        public string AddNodeToMChart(string id, string label, AnxEntityTypeEnum icon = AnxEntityTypeEnum.General, AnxColors color = AnxColors.None, string description = "", DateTime? dateTime = null, byte[] image = null)
        {
            if (!string.IsNullOrEmpty(id))
            {
                id = GenerateTextSHA1(id);
                var idList = MyChart.ChartItemCollection.Where(x => x.Item is End).Select(x => x.Item as End).Where(x => x.Item is Entity).Select(x => x.Item as Entity).FirstOrDefault(x => x.EntityId == id);
                if (idList != null)
                {
                    return id;
                }

                #region Items_Ekleme

                var picture1 = new IconPicture();
                //image = File.ReadAllBytes(@"C:\Publish\murat.png");
                if (image != null)
                {
                    picture1.Data = image;
                    //picture1.DataGuid = Guid.NewGuid().ToString().ToLower();
                    picture1.PictureSizeMethod = PictureSizeMethodEnum.UseCustomSize;
                    picture1.CustomSize = 100D;
                    picture1.DataLength = image.Length;
                    picture1.Visible = true;
                }


                var end1 = new End() { Item = new Entity() { EntityId = id, Identity = id, LabelIsIdentity = false, Item = new Icon() { TextX = 0, TextY = 16, IconStyle = new IconStyle() { IconShadingColourSpecified = (color != AnxColors.None), IconShadingColour = (int)color, Enlargement = IconEnlargementEnum.ICEnlargeSingle, IconPicture = (image == null ? null : picture1), Type = icon.ToString().Replace("7", "-").Replace("8", "(").Replace("9", ")").Replace("_", " ") } } } };
                var item1 = new ChartItem() { Description = description, Label = label, Item = end1, DateTime = (dateTime.HasValue ? dateTime.Value : DateTime.MinValue), DateSet = dateTime.HasValue, DateTimeSpecified = dateTime.HasValue };
                MyChart.ChartItemCollection.Add(item1);
                #endregion
            }
            return id;
        }

        public void AddNodeDetailsToMChart(string id1, string addText, string tip, bool suffix = false)
        {
            if (!string.IsNullOrEmpty(id1) && !string.IsNullOrEmpty(addText))
            {
                if (MyChart.AttributeClassCollection == null)
                {
                    MyChart.AttributeClassCollection = new List<AttributeClass>();
                }
                if (MyChart.AttributeClassCollection.FirstOrDefault(x => x.Name == tip) == null)
                {
                    MyChart.AttributeClassCollection.Add(new AttributeClass() { UserCanAdd = true, Name = tip, Type = AttributeTypeEnum.AttText, Visible = true, ShowValue = true, ShowSuffix = suffix, ShowPrefix = !suffix, Prefix = !suffix ? tip + " " : null, Suffix = suffix ? " " + tip : null });
                }

                var item = MyChart.ChartItemCollection.Where(x => (x.Item is End) && ((x.Item as End).Item is Entity) && ((x.Item as End).Item as Entity).EntityId == id1).FirstOrDefault();
                if (item != null)
                {
                    if (!string.IsNullOrEmpty(addText))
                    {
                        item.AttributeCollection = new List<Attribute>() { new Attribute() { Value = addText, AttributeClass = tip } };
                    }
                }
            }
        }

        public void AddEdgeToMChart(string label, string id1, string id2, AnxColors color = AnxColors.Black, DateTime? dateTime = null)
        {
            #region Link_Ekleme 
            if (!string.IsNullOrEmpty(id1) && !string.IsNullOrEmpty(id2) && id1 != id2)
            {
                var idList = MyChart.ChartItemCollection.Where(x => x.Item is Link).Select(x => x.Item as Link).FirstOrDefault(x => x.End1Id == id1 && x.End2Id == id2);
                if (idList != null)
                {
                    return;
                }

                var link1 = new Link() { End1Id = id1.ToUpper().Trim(), End2Id = id2.ToUpper().Trim(), LinkStyle = new LinkStyle() { Type = "Link", ArrowStyle = ArrowStyleEnum.ArrowOnHead, ArrowStyleSpecified = true, StrengthReference = "Solid", LineColour = (uint)color, LineColourSpecified = true, MlStyle = MultipleLinkStyleEnum.MultiplicityMultiple, MlStyleSpecified = true } };
                var linkItem1 = new ChartItem() { Label = label, Item = link1, DateTime = (dateTime.HasValue ? dateTime.Value : DateTime.MinValue), DateSet = dateTime.HasValue, DateTimeSpecified = dateTime.HasValue };
                MyChart.ChartItemCollection.Add(linkItem1);
            }
            #endregion
        }

        public string ExportMChartToFile(string filePath)
        {
            string resultFile = string.Empty;

            #region CollectionTanımları 
            MyChart.LinkTypeCollection = new List<LinkType>();
            var icons = MyChart.ChartItemCollection.Where(x => x.Item is End).Select(x => x.Item as End).Where(x => x.Item is Entity).Select(x => x.Item as Entity).Where(x => x.Item is Icon).Select(x => x.Item as Icon).Where(x => x.IconStyle != null).Select(x => x.IconStyle.Type).ToList();
            foreach (var item in icons)
            {
                if (MyChart.EntityTypeCollection.FirstOrDefault(x => x.Name == item) == null)
                {
                    MyChart.EntityTypeCollection.Add(new EntityType() { IconFile = item, Colour = 0, Name = item, PreferredRepresentation = PreferredRepresentationEnum.RepresentAsIcon, });
                }
            }
            MyChart.LinkTypeCollection.Add(new LinkType() { Colour = (int)AnxColors.Black, Name = "Link" });
            #endregion            

            XmlSerializer serializer = new XmlSerializer(typeof(Chart));
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.ConformanceLevel = ConformanceLevel.Auto;
            settings.CloseOutput = false;
            settings.Encoding = Encoding.UTF8;

            using (var sw = new StringWriter())
            {
                using (XmlWriter wr = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(wr, MyChart);
                    File.WriteAllText(filePath, sw.ToString());
                    resultFile = filePath;
                }
            }
            return resultFile;
        }

    }

    public enum AnxColors
    {
        None = -1,
        AliceBlue = 16775408,
        AntiqueWhite = 14150650,
        Aqua = 16776960,
        Aquamarine = 13959039,
        Azure = 16777200,
        Beige = 14480885,
        Bisque = 12903679,
        Black = 0,
        BlanchedAlmond = 13495295,
        Blue = 16711680,
        BlueViolet = 14822282,
        Brown = 2763429,
        BurlyWood = 8894686,
        CadetBlue = 10526303,
        Chartreuse = 65407,
        Chocolate = 1993170,
        Coral = 5275647,
        CornflowerBlue = 15570276,
        Cornsilk = 14481663,
        Crimson = 3937500,
        DarkBlue = 9109504,
        DarkCyan = 9145088,
        DarkGoldenrod = 755384,
        DarkGray = 11119017,
        DarkGreen = 100,
        DarkKhaki = 7059389,
        DarkMagenta = 9109643,
        DarkOliveGreen = 3107669,
        DarkOrange = 36095,
        DarkOrchid = 13382297,
        DarkRed = 139,
        DarkSalmon = 8034025,
        DarkSeaGreen = 9157775,
        DarkSlateBlue = 9125192,
        DarkSlateGray = 5197615,
        DarkTurquoise = 13749760,
        DarkViolet = 13828244,
        DeepPink = 9639167,
        DeepSkyBlue = 16760576,
        DimGray = 6908265,
        DodgerBlue = 16748574,
        Firebrick = 2237106,
        FloralWhite = 15792895,
        ForestGreen = 2263842,
        Fuchsia = 16711935,
        Gainsboro = 14474460,
        GhostWhite = 16775416,
        Gold = 55295,
        Goldenrod = 2139610,
        Gray = 8421504,
        Green = 128,
        GreenYellow = 3145645,
        Honeydew = 15794160,
        HotPink = 11823615,
        IndianRed = 6053069,
        Indigo = 8519755,
        Ivory = 15794175,
        Khaki = 9234160,
        Lavender = 16443110,
        LavenderBlush = 16118015,
        LawnGreen = 64636,
        LemonChiffon = 13499135,
        LightBlue = 15128749,
        LightCoral = 8421616,
        LightCyan = 16777184,
        LightGoldenrodYellow = 13826810,
        LightGreen = 9498256,
        LightGray = 13882323,
        LightPink = 12695295,
        LightSalmon = 8036607,
        LightSeaGreen = 11186720,
        LightSkyBlue = 16436871,
        LightSlateGray = 10061943,
        LightSteelBlue = 14599344,
        LightYellow = 14745599,
        Lime = 65280,
        LimeGreen = 3329330,
        Linen = 15134970,
        Maroon = 128,
        MediumAquamarine = 11193702,
        MediumBlue = 13434880,
        MediumOrchid = 13850042,
        MediumPurple = 14381203,
        MediumSeaGreen = 7451452,
        MediumSlateBlue = 15624315,
        MediumSpringGreen = 10156544,
        MediumTurquoise = 13422920,
        MediumVioletRed = 8721863,
        MidnightBlue = 7346457,
        MintCream = 16449525,
        MistyRose = 14804223,
        Moccasin = 11920639,
        NavajoWhite = 11394815,
        Navy = 128,
        OldLace = 15136253,
        Olive = 32896,
        OliveDrab = 2330219,
        Orange = 42495,
        OrangeRed = 17919,
        Orchid = 14053594,
        PaleGoldenrod = 11200750,
        PaleGreen = 10025880,
        PaleTurquoise = 15658671,
        PaleVioletRed = 9662683,
        PapayaWhip = 14020607,
        PeachPuff = 12180223,
        Peru = 4163021,
        Pink = 13353215,
        Plum = 14524637,
        PowderBlue = 15130800,
        Purple = 8388736,
        Red = 255,
        RosyBrown = 9408444,
        RoyalBlue = 26945,
        SaddleBrown = 1262987,
        Salmon = 7504122,
        SandyBrown = 6333684,
        SeaGreen = 5737262,
        SeaShell = 15660543,
        Sienna = 2970272,
        Silver = 12632256,
        SkyBlue = 15453831,
        SlateBlue = 13458026,
        SlateGray = 9470064,
        Snow = 16448255,
        SpringGreen = 8388352,
        SteelBlue = 11829830,
        Tan = 9221330,
        Teal = 32896,
        Thistle = 14204888,
        Tomato = 4678655,
        Turquoise = 13688896,
        Violet = 15631086,
        Wheat = 11788021,
        White = 16777215,
        WhiteSmoke = 16119285,
        Yellow = 65535,
        YellowGreen = 3329434,

    }

    /// <summary>
    /// C:\Program Files (x86)\Common Files\i2 Shared\Images 8.5\Basic\Icons
    /// </summary>
    public enum AnxEntityTypeEnum
    {
        Account,
        Action,
        Administrator,
        Adult_gray,
        Adult_lilac,
        Adult_red,
        Adult_yellow,
        Adult,
        Air_Rifle,
        Aircraft_Carrier,
        Airfield,
        Airplane,
        Alcohol,
        Ambulnc,
        Ambush,
        Anbapp,
        Anbchart,
        Annotation,
        Anomalous_Activity,
        Anon,
        Anonymizer,
        APC,
        Arena,
        Arrest,
        Arson,
        Articab,
        Asltwpn,
        Assassination,
        Assault,
        Atm,
        Attack_Helicopter,
        Audiodoc,
        Backhoe_Loader,
        BankBldg,
        Baseball_Bat,
        Bcase,
        Beaconing,
        Billing_Account,
        Bird,
        Blade_Server,
        Blog,
        Body,
        Bodyguard,
        Bomb_Factory,
        Bomb,
        Bomber,
        Book,
        Bot_Herder,
        Bottle,
        Boy,
        Bridge,
        Brothel,
        Building,
        Bulcases,
        Bulldozer,
        Bullet,
        Bultcase,
        Burglary,
        Cabinet,
        Camcorder,
        Camera,
        Cannabis,
        Car_Bomb,
        Car,
        Case,
        Cash,
        Casino,
        Cassette,
        Cat,
        Category,
        CCTV,
        Cell,
        Cellfone,
        Chart,
        Checbook,
        Checkpoint,
        Cheque,
        Child,
        Chopper,
        Church,
        CirAqua,
        CirBlack,
        CirBlue,
        CirFusch,
        CirGreen,
        CirGrey,
        CirLime,
        CirMaroon,
        CirNavy,
        CirOlive,
        CirPurple,
        CirRed,
        CirSilver,
        CirTeal,
        CirWhite,
        CirYell,
        Claim,
        Clan,
        Classification_pie,
        Clock,
        Clothing,
        Cntrfeit,
        Coach_Station,
        Coach,
        Cocaine,
        Colour_Aqua,
        Colour_Black,
        Colour_Blue,
        Colour_Fuchsia,
        Colour_Green,
        Colour_Grey,
        Colour_Lime,
        Colour_Maroon,
        Colour_Navy,
        Colour_Olive,
        Colour_Purple,
        Colour_Red,
        Colour_Silver,
        Colour_Teal,
        Colour_White,
        Colour_Yellow,
        Command_and_Control,
        Comment,
        Communication,
        Communications_Center,
        Compact_Car,
        Computer_Log,
        Computer_Monitor,
        Computer_Port,
        Computer_Protocol,
        Computer_Switch,
        Computer_Virus,
        Computer_Worm,
        Connected_Network_Cloud,
        Container,
        Contaminated_Letter,
        Contship,
        Counterfeit_Goods,
        Court,
        Cow,
        Crater,
        Credit_Card,
        Credit,
        Crime,
        Criminal_Organization,
        Crowbar,
        Cruiser,
        Cyber_Backdoor,
        Cyber_Exploit,
        Cyber_Implant,
        Cyber_Victim,
        Cycle,
        Data_Breach,
        Data_Cache,
        Data_Exfiltration,
        Data_Leak,
        Data_Package,
        Data_Transfer,
        Database,
        Date,
        DDoS,
        Dealer,
        Deceased_Female,
        Deceased_Male,
        Defaced_Web_Site,
        Destroyer,
        Detonation,
        DiaAqua,
        DiaBlack,
        DiaBlue,
        DiaFusch,
        DiaGreen,
        DiaGrey,
        DiaLime,
        DiaMaroon,
        DiaNavy,
        DiaOlive,
        DiaPurple,
        DiaRed,
        DiaSilver,
        DiaTeal,
        DiaWhite,
        DiaYell,
        Digital_Evidence_Analysis,
        Digital_Evidence,
        Directory,
        Disc,
        Dna,
        Document,
        Dog,
        Dollar,
        Domain_Controller,
        Drivers_License,
        Drug_Exchange,
        Drugs,
        Dump_Truck,
        Ecstasy,
        Electronic_Device,
        Email,
        Entity_vlv,
        Envelopn,
        Event,
        Evidence_Collection,
        Explosion,
        Factory,
        Fake_ID,
        Family,
        Fax_Machine,
        Fax,
        Female_Prison_Officer_US,
        Female_Prison_Officer,
        Female_Prisoner,
        Femaler,
        Femlaw,
        Ferry,
        Fighter,
        Financial_Loan,
        Fire_Truck,
        Firefighter,
        Firewall,
        Flat,
        Flatbed,
        Flight_Arrival,
        Flight_Departure,
        Fngrprnt,
        Footprnt,
        Forensic,
        Frame_Simple_Gradient_Circle,
        Frame_Simple_Gradient_Square,
        Fraud,
        Freightr,
        FTP_Server,
        Fugitive,
        Garage,
        Gaspump,
        General_group,
        General,
        General_7_end,
        General_7_start,
        Generic_Entity_vlv,
        Girl,
        Govrment,
        Graffiti,
        Group_of_People,
        Guerrilla,
        Gun_Emplacement,
        Gun,
        Hacker,
        Hackers_Forum,
        Hackers_Tool,
        Hammer,
        Hand_Grenade,
        Handle,
        Handler_of_Stolen_Goods,
        Hands,
        Hangout,
        Hashtag,
        Hatchback,
        Hazard,
        Hazmat_Responder,
        Heroin,
        Hgv,
        Hgvflat,
        Highway,
        Horse,
        Hospbed,
        Hospital_Crescent,
        Hospital_Diamond,
        Hospital,
        Hotel,
        House,
        Hovcraft,
        ID_Theft,
        Id,
        IED,
        Incident,
        Industrial_Chemicals,
        Industrial_Plant,
        Infected_Computer,
        Inmate,
        Instant_Message_User,
        Instant_Message,
        Instant_Messaging,
        Insurgent,
        Internet_Service_Provider,
        interview,
        IP_Address,
        Jester,
        Jewellry,
        Jiffybag,
        Judgement,
        Keyword,
        Kidnapper,
        Kidnapping,
        Kilo_of_Drugs,
        Knife,
        Lab,
        Lake,
        LAN,
        Laptop,
        Lateral_Movement,
        Learjet,
        Letter_Bomb,
        Letter,
        Licplate,
        Limousine,
        Logic_Bomb,
        Login,
        Lorry,
        MAC_Address,
        Machngun,
        Magnify,
        Maleb,
        Malelaw,
        MANPADS,
        Maritime,
        Massage_Parlor,
        MBRL,
        MBT,
        Mcycle,
        Medic,
        Medical,
        Medlab,
        MedLab2,
        MedLab3,
        Mention,
        Message_Post,
        Message_Posting_User,
        Metal_Pipe,
        Military_Base,
        Minefield,
        Minibus,
        Mnytrans,
        Mobile_Crane,
        Mobile_Trailer,
        Moneybag,
        Mortar,
        Mosque,
        Motor_vehicle,
        Motorhome,
        MP3_Player,
        National_Domain_Registry,
        NATO,
        Network_Access,
        Network_Cloud,
        News_Feed,
        Newspaper_article,
        Notebook,
        Nplate,
        Nuclear,
        Observation_Tower,
        Offense,
        Office_blue,
        Office_green,
        Office_lilac,
        Office_red,
        Office_yellow,
        Office,
        Officer,
        Offndr,
        Offshore_Account,
        Online_Forum,
        Online_Identitiy,
        Online_Marketplace,
        Online_Message,
        Online_Posting,
        OPFOR,
        Organ,
        Organization_Alias,
        Othname,
        Package,
        Packet_Capture,
        Pager,
        Parking_Lot,
        Passport_1,
        Passport,
        Password,
        Patient,
        Patrol_Boat,
        Patrol_Vehicle,
        Patrol,
        Payphone,
        Pc,
        PDA,
        Pepper_Spray,
        Perfume,
        Person,
        Person_8Faceless9,
        Person_8Shaded_Shirt9,
        Phishing,
        Phone,
        Phonebox,
        Photo_Chat_Post,
        Photo_Chat_User,
        Physically_Enabled_Data_Breach,
        Pickup,
        Picturedoc,
        Pills,
        Place,
        Polcar,
        Policcar,
        Police,
        Policewoman_US,
        Policewoman,
        Polisman,
        Polizist,
        Port,
        Post_Office,
        POW,
        Prisoff_German,
        Prisoff_US,
        Prisoff,
        Prison,
        Prisoner,
        Proffeml,
        Profmale,
        Proxy_Server,
        Pub,
        PyrAqua,
        PyrBlack,
        PyrBlue,
        PyrFusch,
        PyrGreen,
        PyrGrey,
        PyrLime,
        PyrMaroon,
        PyrNavy,
        PyrOlive,
        PyrPurple,
        PyrRed,
        PyrSilver,
        PyrTeal,
        PyrWhite,
        PyrYell,
        QRadar_Offense,
        Query,
        Query_ar,
        Radar_Site,
        Radiatn,
        Reconnaissance,
        Repost,
        Restrant,
        Revolver,
        Rifle,
        Road_Block,
        Road,
        Robbery,
        Rocket_Attack,
        Rootkit,
        Rope,
        Router,
        RPG,
        Rx,
        Safe_House,
        Sat_Nav,
        School,
        Screen_Name,
        Search,
        Security_Operation_Center,
        Security_Van,
        Sentence,
        Server_blade,
        Server,
        Shellco,
        Shelter,
        ShieldUK,
        ShieldUS,
        Ship,
        Shopping_Mall,
        Shotgun,
        SIM_Card,
        Small_Boat,
        Snake,
        Sniper,
        Social_Media_Group,
        Social_Media_Photo_Post,
        Social_Media_Post,
        Social_Media_User,
        Social_Media_Video_Post,
        Social_Security_Number,
        Softpira,
        Software,
        Soldier,
        Spammer,
        Spear_Phishing,
        Speeboat,
        Speed_Camera,
        Spoof_IP_Address,
        Spoof_MAC_Address,
        Spoof_Web_Site,
        Sports_Car,
        Spreadsheetdoc,
        Spy,
        Spyware,
        SqrAqua,
        SqrBlack,
        SqrBlue,
        SqrFusch,
        SqrGreen,
        SqrGrey,
        SqrLime,
        SqrMaroon,
        SqrNavy,
        SqrOlive,
        SqrPurple,
        SqrRed,
        SqrSilver,
        SqrTeal,
        SqrWhite,
        SqrYell,
        Srvlanc0,
        Srvlanc1,
        Stage,
        Star,
        Steganography,
        Store,
        StoredQuery,
        Submarine,
        Subscrib,
        Suicide_Bomber,
        Support_Helicopter,
        Switch,
        Taser,
        Taxi,
        Technical_Support,
        Telephone_Mast,
        Telephone_Switchboard,
        Television,
        Terminal,
        Text_Message,
        Textchartviz,
        Threat_Actor,
        Threat_Campaign,
        Tiretred,
        Tobacco,
        Town,
        Trailer,
        Train_Station,
        Train,
        Trash,
        Trawler,
        Trojan,
        UAV,
        Unexploded_Ordnance,
        University,
        USB_Stick,
        User,
        Utility_Vehicle,
        Valuable_Metal,
        Van,
        Vanluton,
        Vanpikup,
        Vcr,
        Vehicle_Insurance_Claim,
        Vidcass,
        Videospool,
        VLAN,
        Voice_Chat,
        VoIP,
        Wahouse,
        Weapons_Cache,
        Web_Guerrilla,
        Web_ID,
        Web_Page,
        Web_Search,
        Web_Server,
        Web_Subscriber,
        Wire_Transfer,
        Wireless_Router,
        Woman,
        Workshop,
        WWW,
        Yacht,

    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ApplicationVersion
    {

        private int majorField;

        private bool majorFieldSpecified;

        private int minorField;

        private bool minorFieldSpecified;

        private int pointField;

        private bool pointFieldSpecified;

        private int buildField;

        private bool buildFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Major
        {
            get
            {
                return this.majorField;
            }
            set
            {
                this.majorField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MajorSpecified
        {
            get
            {
                return this.majorFieldSpecified;
            }
            set
            {
                this.majorFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Minor
        {
            get
            {
                return this.minorField;
            }
            set
            {
                this.minorField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinorSpecified
        {
            get
            {
                return this.minorFieldSpecified;
            }
            set
            {
                this.minorFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Point
        {
            get
            {
                return this.pointField;
            }
            set
            {
                this.pointField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PointSpecified
        {
            get
            {
                return this.pointFieldSpecified;
            }
            set
            {
                this.pointFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Build
        {
            get
            {
                return this.buildField;
            }
            set
            {
                this.buildField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BuildSpecified
        {
            get
            {
                return this.buildFieldSpecified;
            }
            set
            {
                this.buildFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Attribute
    {

        private string attributeClassField;

        private string attributeClassReferenceField;

        private string valueField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AttributeClass
        {
            get
            {
                return this.attributeClassField;
            }
            set
            {
                this.attributeClassField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string AttributeClassReference
        {
            get
            {
                return this.attributeClassReferenceField;
            }
            set
            {
                this.attributeClassReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class AttributeClass
    {

        private List<Font> fontField;

        private int decimalPlacesField;

        private string iconFileField;

        private string idField;

        private bool isUserField;

        private AttributeBehaviourEnum mergeBehaviourField;

        private bool mergeBehaviourFieldSpecified;

        private string nameField;

        private AttributeBehaviourEnum pasteBehaviourField;

        private bool pasteBehaviourFieldSpecified;

        private string prefixField;

        private string semanticTypeGuidField;

        private bool showDateField;

        private bool showIfSetField;

        private bool showPrefixField;

        private bool showSecondsField;

        private bool showSuffixField;

        private bool showSymbolField;

        private bool showTimeField;

        private bool showValueField;

        private string suffixField;

        private AttributeTypeEnum typeField;

        private bool userCanAddField;

        private bool userCanRemoveField;

        private bool visibleField;

        private bool showClassNameField;

        private bool showClassNameFieldSpecified;

        public AttributeClass()
        {
            this.decimalPlacesField = 2;
            this.iconFileField = "General";
            this.isUserField = true;
            this.showDateField = true;
            this.showIfSetField = false;
            this.showPrefixField = false;
            this.showSecondsField = false;
            this.showSuffixField = false;
            this.showSymbolField = true;
            this.showTimeField = true;
            this.showValueField = true;
            this.userCanAddField = true;
            this.userCanRemoveField = true;
            this.visibleField = true;
        }

        [System.Xml.Serialization.XmlElementAttribute("Font")]
        public List<Font> Font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(2)]
        public int DecimalPlaces
        {
            get
            {
                return this.decimalPlacesField;
            }
            set
            {
                this.decimalPlacesField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("General")]
        public string IconFile
        {
            get
            {
                return this.iconFileField;
            }
            set
            {
                this.iconFileField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool IsUser
        {
            get
            {
                return this.isUserField;
            }
            set
            {
                this.isUserField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AttributeBehaviourEnum MergeBehaviour
        {
            get
            {
                return this.mergeBehaviourField;
            }
            set
            {
                this.mergeBehaviourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MergeBehaviourSpecified
        {
            get
            {
                return this.mergeBehaviourFieldSpecified;
            }
            set
            {
                this.mergeBehaviourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AttributeBehaviourEnum PasteBehaviour
        {
            get
            {
                return this.pasteBehaviourField;
            }
            set
            {
                this.pasteBehaviourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PasteBehaviourSpecified
        {
            get
            {
                return this.pasteBehaviourFieldSpecified;
            }
            set
            {
                this.pasteBehaviourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Prefix
        {
            get
            {
                return this.prefixField;
            }
            set
            {
                this.prefixField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string SemanticTypeGuid
        {
            get
            {
                return this.semanticTypeGuidField;
            }
            set
            {
                this.semanticTypeGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ShowDate
        {
            get
            {
                return this.showDateField;
            }
            set
            {
                this.showDateField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowIfSet
        {
            get
            {
                return this.showIfSetField;
            }
            set
            {
                this.showIfSetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowPrefix
        {
            get
            {
                return this.showPrefixField;
            }
            set
            {
                this.showPrefixField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowSeconds
        {
            get
            {
                return this.showSecondsField;
            }
            set
            {
                this.showSecondsField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowSuffix
        {
            get
            {
                return this.showSuffixField;
            }
            set
            {
                this.showSuffixField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ShowSymbol
        {
            get
            {
                return this.showSymbolField;
            }
            set
            {
                this.showSymbolField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ShowTime
        {
            get
            {
                return this.showTimeField;
            }
            set
            {
                this.showTimeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowValue
        {
            get
            {
                return this.showValueField;
            }
            set
            {
                this.showValueField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Suffix
        {
            get
            {
                return this.suffixField;
            }
            set
            {
                this.suffixField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AttributeTypeEnum Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool UserCanAdd
        {
            get
            {
                return this.userCanAddField;
            }
            set
            {
                this.userCanAddField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool UserCanRemove
        {
            get
            {
                return this.userCanRemoveField;
            }
            set
            {
                this.userCanRemoveField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ShowClassName
        {
            get
            {
                return this.showClassNameField;
            }
            set
            {
                this.showClassNameField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ShowClassNameSpecified
        {
            get
            {
                return this.showClassNameFieldSpecified;
            }
            set
            {
                this.showClassNameFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Font
    {

        private int backColourField;

        private bool boldField;

        private FontCharSetEnum charSetField;

        private string faceNameField;

        private int fontColourField;

        private bool italicField;

        private short pointSizeField;

        private bool strikeoutField;

        private bool underlineField;

        public Font()
        {
            this.backColourField = 16777215;
            this.boldField = false;
            this.charSetField = FontCharSetEnum.CharSetDefault;
            this.faceNameField = "Arial";
            this.fontColourField = 0;
            this.italicField = false;
            this.pointSizeField = ((short)(8));
            this.strikeoutField = false;
            this.underlineField = false;
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(16777215)]
        public int BackColour
        {
            get
            {
                return this.backColourField;
            }
            set
            {
                this.backColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Bold
        {
            get
            {
                return this.boldField;
            }
            set
            {
                this.boldField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(FontCharSetEnum.CharSetDefault)]
        public FontCharSetEnum CharSet
        {
            get
            {
                return this.charSetField;
            }
            set
            {
                this.charSetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("Arial")]
        public string FaceName
        {
            get
            {
                return this.faceNameField;
            }
            set
            {
                this.faceNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int FontColour
        {
            get
            {
                return this.fontColourField;
            }
            set
            {
                this.fontColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Italic
        {
            get
            {
                return this.italicField;
            }
            set
            {
                this.italicField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(typeof(short), "8")]
        public short PointSize
        {
            get
            {
                return this.pointSizeField;
            }
            set
            {
                this.pointSizeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Strikeout
        {
            get
            {
                return this.strikeoutField;
            }
            set
            {
                this.strikeoutField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Underline
        {
            get
            {
                return this.underlineField;
            }
            set
            {
                this.underlineField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum FontCharSetEnum
    {

        /// <remarks/>
        CharSetANSI,

        /// <remarks/>
        CharSetArabic,

        /// <remarks/>
        CharSetBaltic,

        /// <remarks/>
        CharSetChineseBig5,

        /// <remarks/>
        CharSetDefault,

        /// <remarks/>
        CharSetEastEuropean,

        /// <remarks/>
        CharSetGB2312,

        /// <remarks/>
        CharSetGreek,

        /// <remarks/>
        CharSetHangul,

        /// <remarks/>
        CharSetHebrew,

        /// <remarks/>
        CharSetJohab,

        /// <remarks/>
        CharSetMAC,

        /// <remarks/>
        CharSetOEM,

        /// <remarks/>
        CharSetRussian,

        /// <remarks/>
        CharSetShiftJIS,

        /// <remarks/>
        CharSetSymbol,

        /// <remarks/>
        CharSetThai,

        /// <remarks/>
        CharSetTurkish,

        /// <remarks/>
        CharSetVietnamese,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum AttributeBehaviourEnum
    {

        /// <remarks/>
        AttMergeAdd,

        /// <remarks/>
        AttMergeAddIfNotIn,

        /// <remarks/>
        AttMergeAddWithLineBreak,

        /// <remarks/>
        AttMergeAddWithLineBreakIfNotIn,

        /// <remarks/>
        AttMergeAddWithSpace,

        /// <remarks/>
        AttMergeAddWithSpaceIfNotIn,

        /// <remarks/>
        AttMergeAND,

        /// <remarks/>
        AttMergeAssign,

        /// <remarks/>
        AttMergeMax,

        /// <remarks/>
        AttMergeMin,

        /// <remarks/>
        AttMergeNoOp,

        /// <remarks/>
        AttMergeOR,

        /// <remarks/>
        AttMergeSubtract,

        /// <remarks/>
        AttMergeSubtractSwap,

        /// <remarks/>
        AttMergeXOR,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum AttributeTypeEnum
    {

        /// <remarks/>
        AttFlag,

        /// <remarks/>
        AttNumber,

        /// <remarks/>
        AttText,

        /// <remarks/>
        AttTime,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class AttributeClassCollection
    {

        private List<AttributeClass> attributeClassField;

        [System.Xml.Serialization.XmlElementAttribute("AttributeClass")]
        public List<AttributeClass> AttributeClass
        {
            get
            {
                return this.attributeClassField;
            }
            set
            {
                this.attributeClassField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class AttributeClassEntry
    {

        private string attributeClassField;

        private string attributeClassReferenceField;

        private string valueField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string AttributeClass
        {
            get
            {
                return this.attributeClassField;
            }
            set
            {
                this.attributeClassField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string AttributeClassReference
        {
            get
            {
                return this.attributeClassReferenceField;
            }
            set
            {
                this.attributeClassReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class AttributeClassEntryCollection
    {

        private List<AttributeClassEntry> attributeClassEntryField;

        [System.Xml.Serialization.XmlElementAttribute("AttributeClassEntry")]
        public List<AttributeClassEntry> AttributeClassEntry
        {
            get
            {
                return this.attributeClassEntryField;
            }
            set
            {
                this.attributeClassEntryField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class AttributeCollection
    {

        private List<Attribute> attributeField;

        [System.Xml.Serialization.XmlElementAttribute("Attribute")]
        public List<Attribute> Attribute
        {
            get
            {
                return this.attributeField;
            }
            set
            {
                this.attributeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class AttributeEntryCollection
    {

        private List<AttributeClassEntry> attributeClassEntryField;

        [System.Xml.Serialization.XmlElementAttribute("AttributeClassEntry")]
        public List<AttributeClassEntry> AttributeClassEntry
        {
            get
            {
                return this.attributeClassEntryField;
            }
            set
            {
                this.attributeClassEntryField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Box
    {

        private BoxStyle boxStyleField;

        private int depthField;

        private int heightField;

        private int textXField;

        private int textYField;

        private int widthField;

        public Box()
        {
            this.depthField = 0;
            this.heightField = 100;
            this.textXField = 2;
            this.textYField = 1;
            this.widthField = 100;
        }

        public BoxStyle BoxStyle
        {
            get
            {
                return this.boxStyleField;
            }
            set
            {
                this.boxStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Depth
        {
            get
            {
                return this.depthField;
            }
            set
            {
                this.depthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(100)]
        public int Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(2)]
        public int TextX
        {
            get
            {
                return this.textXField;
            }
            set
            {
                this.textXField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(1)]
        public int TextY
        {
            get
            {
                return this.textYField;
            }
            set
            {
                this.textYField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(100)]
        public int Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class BoxStyle
    {

        private int backColourField;

        private bool backColourFieldSpecified;

        private string entityTypeReferenceField;

        private bool filledField;

        private bool filledFieldSpecified;

        private FillStyleEnum fillStyleField;

        private bool fillStyleFieldSpecified;

        private int lineWidthField;

        private bool lineWidthFieldSpecified;

        private uint lineColourField;

        private bool lineColourFieldSpecified;

        private string strengthField;

        private string strengthReferenceField;

        private string typeField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BackColour
        {
            get
            {
                return this.backColourField;
            }
            set
            {
                this.backColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BackColourSpecified
        {
            get
            {
                return this.backColourFieldSpecified;
            }
            set
            {
                this.backColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Filled
        {
            get
            {
                return this.filledField;
            }
            set
            {
                this.filledField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FilledSpecified
        {
            get
            {
                return this.filledFieldSpecified;
            }
            set
            {
                this.filledFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FillStyleEnum FillStyle
        {
            get
            {
                return this.fillStyleField;
            }
            set
            {
                this.fillStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillStyleSpecified
        {
            get
            {
                return this.fillStyleFieldSpecified;
            }
            set
            {
                this.fillStyleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineWidthSpecified
        {
            get
            {
                return this.lineWidthFieldSpecified;
            }
            set
            {
                this.lineWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineColourSpecified
        {
            get
            {
                return this.lineColourFieldSpecified;
            }
            set
            {
                this.lineColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum FillStyleEnum
    {

        /// <remarks/>
        Transparent,

        /// <remarks/>
        Gradient,

        /// <remarks/>
        Solid,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Card
    {

        private TimeZone timeZoneField;

        private bool dateSetField;

        private System.DateTime dateTimeField;

        private bool dateTimeFieldSpecified;

        private string dateTimeDescriptionField;

        private int gradeOneIndexField;

        private string gradeOneReferenceField;

        private int gradeThreeIndexField;

        private string gradeThreeReferenceField;

        private int gradeTwoIndexField;

        private string gradeTwoReferenceField;

        private string localDateTimeOffsetField;

        private string sourceReferenceField;

        private string sourceTypeField;

        private string summaryField;

        private string textField;

        private bool timeSetField;

        public Card()
        {
            this.dateSetField = false;
            this.gradeOneIndexField = 0;
            this.gradeThreeIndexField = 0;
            this.gradeTwoIndexField = 0;
            this.timeSetField = false;
        }

        public TimeZone TimeZone
        {
            get
            {
                return this.timeZoneField;
            }
            set
            {
                this.timeZoneField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool DateSet
        {
            get
            {
                return this.dateSetField;
            }
            set
            {
                this.dateSetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime DateTime
        {
            get
            {
                return this.dateTimeField;
            }
            set
            {
                this.dateTimeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DateTimeSpecified
        {
            get
            {
                return this.dateTimeFieldSpecified;
            }
            set
            {
                this.dateTimeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DateTimeDescription
        {
            get
            {
                return this.dateTimeDescriptionField;
            }
            set
            {
                this.dateTimeDescriptionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int GradeOneIndex
        {
            get
            {
                return this.gradeOneIndexField;
            }
            set
            {
                this.gradeOneIndexField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GradeOneReference
        {
            get
            {
                return this.gradeOneReferenceField;
            }
            set
            {
                this.gradeOneReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int GradeThreeIndex
        {
            get
            {
                return this.gradeThreeIndexField;
            }
            set
            {
                this.gradeThreeIndexField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GradeThreeReference
        {
            get
            {
                return this.gradeThreeReferenceField;
            }
            set
            {
                this.gradeThreeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int GradeTwoIndex
        {
            get
            {
                return this.gradeTwoIndexField;
            }
            set
            {
                this.gradeTwoIndexField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GradeTwoReference
        {
            get
            {
                return this.gradeTwoReferenceField;
            }
            set
            {
                this.gradeTwoReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string LocalDateTimeOffset
        {
            get
            {
                return this.localDateTimeOffsetField;
            }
            set
            {
                this.localDateTimeOffsetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SourceReference
        {
            get
            {
                return this.sourceReferenceField;
            }
            set
            {
                this.sourceReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SourceType
        {
            get
            {
                return this.sourceTypeField;
            }
            set
            {
                this.sourceTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Summary
        {
            get
            {
                return this.summaryField;
            }
            set
            {
                this.summaryField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool TimeSet
        {
            get
            {
                return this.timeSetField;
            }
            set
            {
                this.timeSetField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TimeZone
    {

        private string nameField;

        private int uniqueIDField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int UniqueID
        {
            get
            {
                return this.uniqueIDField;
            }
            set
            {
                this.uniqueIDField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CardCollection
    {

        private List<Card> cardField;

        [System.Xml.Serialization.XmlElementAttribute("Card")]
        public List<Card> Card
        {
            get
            {
                return this.cardField;
            }
            set
            {
                this.cardField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CustomImageCollection
    {

        private List<CustomImage> customImageField;

        [System.Xml.Serialization.XmlElementAttribute("CustomImage")]
        public List<CustomImage> CustomImage
        {
            get
            {
                return this.customImageField;
            }
            set
            {
                this.customImageField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CustomImage
    {

        private string idField;

        private int dataLengthField;

        private bool dataLengthFieldSpecified;

        private string dataGuidField;

        private byte[] dataField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int DataLength
        {
            get
            {
                return this.dataLengthField;
            }
            set
            {
                this.dataLengthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DataLengthSpecified
        {
            get
            {
                return this.dataLengthFieldSpecified;
            }
            set
            {
                this.dataLengthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DataGuid
        {
            get
            {
                return this.dataGuidField;
            }
            set
            {
                this.dataGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "base64Binary")]
        public byte[] Data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PropertyBagCollection
    {

        private List<PropertyBag> propertyBagField;

        [System.Xml.Serialization.XmlElementAttribute("PropertyBag")]
        public List<PropertyBag> PropertyBag
        {
            get
            {
                return this.propertyBagField;
            }
            set
            {
                this.propertyBagField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PropertyBag
    {

        private List<PropertyBagProperty> propertyBagPropertyField;

        private string guidIDField;

        [System.Xml.Serialization.XmlElementAttribute("PropertyBagProperty")]
        public List<PropertyBagProperty> PropertyBagProperty
        {
            get
            {
                return this.propertyBagPropertyField;
            }
            set
            {
                this.propertyBagPropertyField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GuidID
        {
            get
            {
                return this.guidIDField;
            }
            set
            {
                this.guidIDField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PropertyBagProperty
    {

        private string propertyNameField;

        private PropertyBagDataType dataTypeField;

        private bool dataTypeFieldSpecified;

        private bool valueIsIDispatchField;

        private bool valueIsIDispatchFieldSpecified;

        private int dataLengthField;

        private bool dataLengthFieldSpecified;

        private string dataGuidField;

        private string dataField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PropertyName
        {
            get
            {
                return this.propertyNameField;
            }
            set
            {
                this.propertyNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PropertyBagDataType DataType
        {
            get
            {
                return this.dataTypeField;
            }
            set
            {
                this.dataTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DataTypeSpecified
        {
            get
            {
                return this.dataTypeFieldSpecified;
            }
            set
            {
                this.dataTypeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ValueIsIDispatch
        {
            get
            {
                return this.valueIsIDispatchField;
            }
            set
            {
                this.valueIsIDispatchField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ValueIsIDispatchSpecified
        {
            get
            {
                return this.valueIsIDispatchFieldSpecified;
            }
            set
            {
                this.valueIsIDispatchFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int DataLength
        {
            get
            {
                return this.dataLengthField;
            }
            set
            {
                this.dataLengthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DataLengthSpecified
        {
            get
            {
                return this.dataLengthFieldSpecified;
            }
            set
            {
                this.dataLengthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DataGuid
        {
            get
            {
                return this.dataGuidField;
            }
            set
            {
                this.dataGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum PropertyBagDataType
    {

        /// <remarks/>
        Boolean,

        /// <remarks/>
        DateTime,

        /// <remarks/>
        Decimal,

        /// <remarks/>
        Double,

        /// <remarks/>
        Integer,

        /// <remarks/>
        String,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Chart
    {

        private List<ApplicationVersion> applicationVersionField;

        private List<LibraryCatalogueType> libraryCatalogueField;

        private List<PropertyBagCollection> propertyBagCollectionField;

        private List<CustomImage> customImageCollectionField;

        private List<GradeOne> gradeOneField;

        private List<GradeTwo> gradeTwoField;

        private List<GradeThree> gradeThreeField;

        private List<SourceHints> sourceHintsField;

        private List<Strength> strengthCollectionField;

        private List<AttributeClass> attributeClassCollectionField;

        private List<EntityType> entityTypeCollectionField;

        private List<LinkType> linkTypeCollectionField;

        private List<Font> fontField;

        private List<DatabaseProxy> databaseProxyCollectionField;

        private List<DateTimeFormat> dateTimeFormatCollectionField;

        private List<CurrentStyleCollection> currentStyleCollectionField;

        private List<TimeZone> timeZoneField;

        private List<TimeBar> timeBarField;

        private List<PrintSettings> printSettingsField;

        private List<Summary> summaryField;

        private List<ChartItem> chartItemCollectionField;

        private List<Connection> connectionCollectionField;

        private List<ThemeJunctions> junctionCollectionField;

        private List<Group> groupCollectionField;

        private List<Palette> paletteCollectionField;

        private List<PaletteBar> paletteBarCollectionField;

        private List<LegendDefinition> legendDefinitionField;

        private List<Snapshot> snapshotCollectionField;

        private string schemaVersionField;

        private string msxmlVersionField;

        private int backColourField;

        private bool blankLinkLabelsField;

        private System.DateTime defaultDateField;

        private bool defaultDateFieldSpecified;

        private System.DateTime defaultDateTimeForNewChartField;

        private bool defaultDateTimeForNewChartFieldSpecified;

        private double defaultLinkSpacingField;

        private double defaultTickRateField;

        private double gridHeightSizeField;

        private bool gridVisibleOnAllViewsField;

        private double gridWidthSizeField;

        private bool hideMatchingTimeZoneFormatField;

        private bool idReferenceLinkingField;

        private bool isBackColourFilledField;

        private LabelMergeAndPasteRuleEnum labelRuleField;

        private bool labelSumNumericLinksField;

        private bool rigorousField;

        private bool showAllFlagField;

        private HiddenItemsVisibilityEnum hiddenItemsVisibilityField;

        private bool showPagesField;

        private bool snapToGridField;

        private bool timeBarVisibleField;

        private TypeIconDrawingModeEnum typeIconDrawingModeField;

        private bool useDefaultLinkSpacingWhenDraggingField;

        private bool useLocalTimeZoneField;

        private bool useWiringHeightForThemeIconField;

        private double wiringDistanceFarField;

        private double wiringDistanceNearField;

        private double wiringHeightField;

        private double wiringSpacingField;

        private bool coverSheetShowOnOpenField;

        private bool coverSheetShowOnOpenFieldSpecified;

        public Chart()
        {
            this.schemaVersionField = "7.0.0.1";
            this.msxmlVersionField = "4.20.9818.0";
            this.backColourField = 16777215;
            this.blankLinkLabelsField = false;
            this.defaultLinkSpacingField = 0.295275590551181D;
            this.defaultTickRateField = 1D;
            this.gridHeightSizeField = 0.295275590551181D;
            this.gridVisibleOnAllViewsField = true;
            this.gridWidthSizeField = 0.295275590551181D;
            this.hideMatchingTimeZoneFormatField = false;
            this.idReferenceLinkingField = true;
            this.isBackColourFilledField = true;
            this.labelRuleField = LabelMergeAndPasteRuleEnum.LabelRuleMerge;
            this.labelSumNumericLinksField = false;
            this.rigorousField = true;
            this.showAllFlagField = false;
            this.hiddenItemsVisibilityField = HiddenItemsVisibilityEnum.ItemsVisibilityHidden;
            this.showPagesField = false;
            this.snapToGridField = false;
            this.timeBarVisibleField = false;
            this.typeIconDrawingModeField = TypeIconDrawingModeEnum.HighQuality;
            this.useDefaultLinkSpacingWhenDraggingField = false;
            this.useLocalTimeZoneField = true;
            this.useWiringHeightForThemeIconField = true;
            this.wiringDistanceFarField = 0.393700787401575D;
            this.wiringDistanceNearField = 0.078740157480315D;
            this.wiringHeightField = 0.196850393700787D;
            this.wiringSpacingField = 0.196850393700787D;
        }

        [System.Xml.Serialization.XmlElementAttribute("ApplicationVersion")]
        public List<ApplicationVersion> ApplicationVersion
        {
            get
            {
                return this.applicationVersionField;
            }
            set
            {
                this.applicationVersionField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("LibraryCatalogue", Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
        public List<LibraryCatalogueType> LibraryCatalogue
        {
            get
            {
                return this.libraryCatalogueField;
            }
            set
            {
                this.libraryCatalogueField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("PropertyBagCollection")]
        public List<PropertyBagCollection> PropertyBagCollection
        {
            get
            {
                return this.propertyBagCollectionField;
            }
            set
            {
                this.propertyBagCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("CustomImage", typeof(CustomImage), IsNullable = false)]
        public List<CustomImage> CustomImageCollection
        {
            get
            {
                return this.customImageCollectionField;
            }
            set
            {
                this.customImageCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("GradeOne")]
        public List<GradeOne> GradeOne
        {
            get
            {
                return this.gradeOneField;
            }
            set
            {
                this.gradeOneField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("GradeTwo")]
        public List<GradeTwo> GradeTwo
        {
            get
            {
                return this.gradeTwoField;
            }
            set
            {
                this.gradeTwoField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("GradeThree")]
        public List<GradeThree> GradeThree
        {
            get
            {
                return this.gradeThreeField;
            }
            set
            {
                this.gradeThreeField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("SourceHints")]
        public List<SourceHints> SourceHints
        {
            get
            {
                return this.sourceHintsField;
            }
            set
            {
                this.sourceHintsField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Strength", typeof(Strength), IsNullable = false)]
        public List<Strength> StrengthCollection
        {
            get
            {
                return this.strengthCollectionField;
            }
            set
            {
                this.strengthCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("AttributeClass", typeof(AttributeClass), IsNullable = false)]
        public List<AttributeClass> AttributeClassCollection
        {
            get
            {
                return this.attributeClassCollectionField;
            }
            set
            {
                this.attributeClassCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("EntityType", typeof(EntityType), IsNullable = false)]
        public List<EntityType> EntityTypeCollection
        {
            get
            {
                return this.entityTypeCollectionField;
            }
            set
            {
                this.entityTypeCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("LinkType", typeof(LinkType), IsNullable = false)]
        public List<LinkType> LinkTypeCollection
        {
            get
            {
                return this.linkTypeCollectionField;
            }
            set
            {
                this.linkTypeCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Font")]
        public List<Font> Font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("DatabaseProxy", typeof(DatabaseProxy), IsNullable = false)]
        public List<DatabaseProxy> DatabaseProxyCollection
        {
            get
            {
                return this.databaseProxyCollectionField;
            }
            set
            {
                this.databaseProxyCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("DateTimeFormat", typeof(DateTimeFormat), IsNullable = false)]
        public List<DateTimeFormat> DateTimeFormatCollection
        {
            get
            {
                return this.dateTimeFormatCollectionField;
            }
            set
            {
                this.dateTimeFormatCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("CurrentStyleCollection")]
        public List<CurrentStyleCollection> CurrentStyleCollection
        {
            get
            {
                return this.currentStyleCollectionField;
            }
            set
            {
                this.currentStyleCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("TimeZone")]
        public List<TimeZone> TimeZone
        {
            get
            {
                return this.timeZoneField;
            }
            set
            {
                this.timeZoneField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("TimeBar")]
        public List<TimeBar> TimeBar
        {
            get
            {
                return this.timeBarField;
            }
            set
            {
                this.timeBarField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("PrintSettings")]
        public List<PrintSettings> PrintSettings
        {
            get
            {
                return this.printSettingsField;
            }
            set
            {
                this.printSettingsField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Summary")]
        public List<Summary> Summary
        {
            get
            {
                return this.summaryField;
            }
            set
            {
                this.summaryField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("ChartItem", typeof(ChartItem), IsNullable = false)]
        public List<ChartItem> ChartItemCollection
        {
            get
            {
                return this.chartItemCollectionField;
            }
            set
            {
                this.chartItemCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Connection", typeof(Connection), IsNullable = false)]
        public List<Connection> ConnectionCollection
        {
            get
            {
                return this.connectionCollectionField;
            }
            set
            {
                this.connectionCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("ThemeJunctions", typeof(ThemeJunctions), IsNullable = false)]
        public List<ThemeJunctions> JunctionCollection
        {
            get
            {
                return this.junctionCollectionField;
            }
            set
            {
                this.junctionCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Group", typeof(Group), IsNullable = false)]
        public List<Group> GroupCollection
        {
            get
            {
                return this.groupCollectionField;
            }
            set
            {
                this.groupCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Palette", typeof(Palette), IsNullable = false)]
        public List<Palette> PaletteCollection
        {
            get
            {
                return this.paletteCollectionField;
            }
            set
            {
                this.paletteCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("PaletteBar", typeof(PaletteBar), IsNullable = false)]
        public List<PaletteBar> PaletteBarCollection
        {
            get
            {
                return this.paletteBarCollectionField;
            }
            set
            {
                this.paletteBarCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("LegendDefinition")]
        public List<LegendDefinition> LegendDefinition
        {
            get
            {
                return this.legendDefinitionField;
            }
            set
            {
                this.legendDefinitionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Snapshot", typeof(Snapshot), IsNullable = false)]
        public List<Snapshot> SnapshotCollection
        {
            get
            {
                return this.snapshotCollectionField;
            }
            set
            {
                this.snapshotCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("7.0.0.1")]
        public string SchemaVersion
        {
            get
            {
                return this.schemaVersionField;
            }
            set
            {
                this.schemaVersionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("4.20.9818.0")]
        public string MsxmlVersion
        {
            get
            {
                return this.msxmlVersionField;
            }
            set
            {
                this.msxmlVersionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(16777215)]
        public int BackColour
        {
            get
            {
                return this.backColourField;
            }
            set
            {
                this.backColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool BlankLinkLabels
        {
            get
            {
                return this.blankLinkLabelsField;
            }
            set
            {
                this.blankLinkLabelsField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime DefaultDate
        {
            get
            {
                return this.defaultDateField;
            }
            set
            {
                this.defaultDateField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DefaultDateSpecified
        {
            get
            {
                return this.defaultDateFieldSpecified;
            }
            set
            {
                this.defaultDateFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime DefaultDateTimeForNewChart
        {
            get
            {
                return this.defaultDateTimeForNewChartField;
            }
            set
            {
                this.defaultDateTimeForNewChartField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DefaultDateTimeForNewChartSpecified
        {
            get
            {
                return this.defaultDateTimeForNewChartFieldSpecified;
            }
            set
            {
                this.defaultDateTimeForNewChartFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0.295275590551181D)]
        public double DefaultLinkSpacing
        {
            get
            {
                return this.defaultLinkSpacingField;
            }
            set
            {
                this.defaultLinkSpacingField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(1D)]
        public double DefaultTickRate
        {
            get
            {
                return this.defaultTickRateField;
            }
            set
            {
                this.defaultTickRateField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0.295275590551181D)]
        public double GridHeightSize
        {
            get
            {
                return this.gridHeightSizeField;
            }
            set
            {
                this.gridHeightSizeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool GridVisibleOnAllViews
        {
            get
            {
                return this.gridVisibleOnAllViewsField;
            }
            set
            {
                this.gridVisibleOnAllViewsField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0.295275590551181D)]
        public double GridWidthSize
        {
            get
            {
                return this.gridWidthSizeField;
            }
            set
            {
                this.gridWidthSizeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool HideMatchingTimeZoneFormat
        {
            get
            {
                return this.hideMatchingTimeZoneFormatField;
            }
            set
            {
                this.hideMatchingTimeZoneFormatField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool IdReferenceLinking
        {
            get
            {
                return this.idReferenceLinkingField;
            }
            set
            {
                this.idReferenceLinkingField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool IsBackColourFilled
        {
            get
            {
                return this.isBackColourFilledField;
            }
            set
            {
                this.isBackColourFilledField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(LabelMergeAndPasteRuleEnum.LabelRuleMerge)]
        public LabelMergeAndPasteRuleEnum LabelRule
        {
            get
            {
                return this.labelRuleField;
            }
            set
            {
                this.labelRuleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool LabelSumNumericLinks
        {
            get
            {
                return this.labelSumNumericLinksField;
            }
            set
            {
                this.labelSumNumericLinksField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool Rigorous
        {
            get
            {
                return this.rigorousField;
            }
            set
            {
                this.rigorousField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowAllFlag
        {
            get
            {
                return this.showAllFlagField;
            }
            set
            {
                this.showAllFlagField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(HiddenItemsVisibilityEnum.ItemsVisibilityHidden)]
        public HiddenItemsVisibilityEnum HiddenItemsVisibility
        {
            get
            {
                return this.hiddenItemsVisibilityField;
            }
            set
            {
                this.hiddenItemsVisibilityField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool ShowPages
        {
            get
            {
                return this.showPagesField;
            }
            set
            {
                this.showPagesField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool SnapToGrid
        {
            get
            {
                return this.snapToGridField;
            }
            set
            {
                this.snapToGridField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool TimeBarVisible
        {
            get
            {
                return this.timeBarVisibleField;
            }
            set
            {
                this.timeBarVisibleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(TypeIconDrawingModeEnum.HighQuality)]
        public TypeIconDrawingModeEnum TypeIconDrawingMode
        {
            get
            {
                return this.typeIconDrawingModeField;
            }
            set
            {
                this.typeIconDrawingModeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool UseDefaultLinkSpacingWhenDragging
        {
            get
            {
                return this.useDefaultLinkSpacingWhenDraggingField;
            }
            set
            {
                this.useDefaultLinkSpacingWhenDraggingField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool UseLocalTimeZone
        {
            get
            {
                return this.useLocalTimeZoneField;
            }
            set
            {
                this.useLocalTimeZoneField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool UseWiringHeightForThemeIcon
        {
            get
            {
                return this.useWiringHeightForThemeIconField;
            }
            set
            {
                this.useWiringHeightForThemeIconField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0.393700787401575D)]
        public double WiringDistanceFar
        {
            get
            {
                return this.wiringDistanceFarField;
            }
            set
            {
                this.wiringDistanceFarField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0.078740157480315D)]
        public double WiringDistanceNear
        {
            get
            {
                return this.wiringDistanceNearField;
            }
            set
            {
                this.wiringDistanceNearField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0.196850393700787D)]
        public double WiringHeight
        {
            get
            {
                return this.wiringHeightField;
            }
            set
            {
                this.wiringHeightField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0.196850393700787D)]
        public double WiringSpacing
        {
            get
            {
                return this.wiringSpacingField;
            }
            set
            {
                this.wiringSpacingField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool CoverSheetShowOnOpen
        {
            get
            {
                return this.coverSheetShowOnOpenField;
            }
            set
            {
                this.coverSheetShowOnOpenField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CoverSheetShowOnOpenSpecified
        {
            get
            {
                return this.coverSheetShowOnOpenFieldSpecified;
            }
            set
            {
                this.coverSheetShowOnOpenFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class LibraryCatalogueType
    {

        private List<TypeType> typeField;

        private List<PropertyType> propertyField;

        private List<FormType> formField;

        private List<DomainType> domainField;

        private List<SectorDefinition> sectorDefinitionField;

        private string versionMajorField;

        private string versionMinorField;

        private string versionReleaseField;

        private string versionBuildField;

        private string localeHexField;

        private string localeVersionField;

        [System.Xml.Serialization.XmlElementAttribute("Type")]
        public List<TypeType> Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Property")]
        public List<PropertyType> Property
        {
            get
            {
                return this.propertyField;
            }
            set
            {
                this.propertyField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Form")]
        public List<FormType> Form
        {
            get
            {
                return this.formField;
            }
            set
            {
                this.formField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Domain")]
        public List<DomainType> Domain
        {
            get
            {
                return this.domainField;
            }
            set
            {
                this.domainField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("SectorDefinition")]
        public List<SectorDefinition> SectorDefinition
        {
            get
            {
                return this.sectorDefinitionField;
            }
            set
            {
                this.sectorDefinitionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string VersionMajor
        {
            get
            {
                return this.versionMajorField;
            }
            set
            {
                this.versionMajorField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string VersionMinor
        {
            get
            {
                return this.versionMinorField;
            }
            set
            {
                this.versionMinorField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string VersionRelease
        {
            get
            {
                return this.versionReleaseField;
            }
            set
            {
                this.versionReleaseField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string VersionBuild
        {
            get
            {
                return this.versionBuildField;
            }
            set
            {
                this.versionBuildField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LocaleHex
        {
            get
            {
                return this.localeHexField;
            }
            set
            {
                this.localeHexField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string LocaleVersion
        {
            get
            {
                return this.localeVersionField;
            }
            set
            {
                this.localeVersionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class TypeType
    {

        private string typeNameField;

        private List<string> imageFileField;

        private List<TypeTypeLNMapping> lNMappingField;

        private List<string> sectorField;

        private DocumentationType documentationField;

        private string tGUIDField;

        private string kindOfField;

        private bool abstractField;

        private bool isSymmetricLinkField;

        private bool isSymmetricLinkFieldSpecified;

        public TypeType()
        {
            this.abstractField = false;
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TypeName
        {
            get
            {
                return this.typeNameField;
            }
            set
            {
                this.typeNameField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("ImageFile", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<string> ImageFile
        {
            get
            {
                return this.imageFileField;
            }
            set
            {
                this.imageFileField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("LNMapping", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<TypeTypeLNMapping> LNMapping
        {
            get
            {
                return this.lNMappingField;
            }
            set
            {
                this.lNMappingField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Sector", DataType = "IDREF")]
        public List<string> Sector
        {
            get
            {
                return this.sectorField;
            }
            set
            {
                this.sectorField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DocumentationType Documentation
        {
            get
            {
                return this.documentationField;
            }
            set
            {
                this.documentationField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string tGUID
        {
            get
            {
                return this.tGUIDField;
            }
            set
            {
                this.tGUIDField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string kindOf
        {
            get
            {
                return this.kindOfField;
            }
            set
            {
                this.kindOfField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool @abstract
        {
            get
            {
                return this.abstractField;
            }
            set
            {
                this.abstractField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool isSymmetricLink
        {
            get
            {
                return this.isSymmetricLinkField;
            }
            set
            {
                this.isSymmetricLinkField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isSymmetricLinkSpecified
        {
            get
            {
                return this.isSymmetricLinkFieldSpecified;
            }
            set
            {
                this.isSymmetricLinkFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class TypeTypeLNMapping
    {

        private string iconFileField;

        private string typeNameField;

        private string localeField;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string IconFile
        {
            get
            {
                return this.iconFileField;
            }
            set
            {
                this.iconFileField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TypeName
        {
            get
            {
                return this.typeNameField;
            }
            set
            {
                this.typeNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string locale
        {
            get
            {
                return this.localeField;
            }
            set
            {
                this.localeField = value;
            }
        }
    }

    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleDocumentationType))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class DocumentationType
    {

        private string rationaleField;

        private List<string> synonymField;

        private string descriptionField;

        private Status statusField;

        private bool statusFieldSpecified;

        public string Rationale
        {
            get
            {
                return this.rationaleField;
            }
            set
            {
                this.rationaleField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Synonym")]
        public List<string> Synonym
        {
            get
            {
                return this.synonymField;
            }
            set
            {
                this.synonymField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public Status status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool statusSpecified
        {
            get
            {
                return this.statusFieldSpecified;
            }
            set
            {
                this.statusFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public enum Status
    {

        /// <remarks/>
        Proposed,

        /// <remarks/>
        Pending,

        /// <remarks/>
        Rejected,

        /// <remarks/>
        Approved,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class SimpleDocumentationType : DocumentationType
    {
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class PropertyType
    {

        private string propertyNameField;

        private List<PropertyTypeRelatedType> relatedTypeField;

        private List<PropertyTypeUnrelatedType> unrelatedTypeField;

        private List<PropertyTypeForm> formField;

        private List<PropertyTypeUnrelatedForm> unrelatedFormField;

        private List<string> sectorField;

        private DocumentationType documentationField;

        private string pGUIDField;

        private string basePropertyField;

        private bool abstractField;

        public PropertyType()
        {
            this.abstractField = false;
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PropertyName
        {
            get
            {
                return this.propertyNameField;
            }
            set
            {
                this.propertyNameField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("RelatedType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<PropertyTypeRelatedType> RelatedType
        {
            get
            {
                return this.relatedTypeField;
            }
            set
            {
                this.relatedTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("UnrelatedType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<PropertyTypeUnrelatedType> UnrelatedType
        {
            get
            {
                return this.unrelatedTypeField;
            }
            set
            {
                this.unrelatedTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Form", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<PropertyTypeForm> Form
        {
            get
            {
                return this.formField;
            }
            set
            {
                this.formField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("UnrelatedForm", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<PropertyTypeUnrelatedForm> UnrelatedForm
        {
            get
            {
                return this.unrelatedFormField;
            }
            set
            {
                this.unrelatedFormField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("Sector", DataType = "IDREF")]
        public List<string> Sector
        {
            get
            {
                return this.sectorField;
            }
            set
            {
                this.sectorField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DocumentationType Documentation
        {
            get
            {
                return this.documentationField;
            }
            set
            {
                this.documentationField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string pGUID
        {
            get
            {
                return this.pGUIDField;
            }
            set
            {
                this.pGUIDField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string baseProperty
        {
            get
            {
                return this.basePropertyField;
            }
            set
            {
                this.basePropertyField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool @abstract
        {
            get
            {
                return this.abstractField;
            }
            set
            {
                this.abstractField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class PropertyTypeRelatedType
    {

        private string tGUIDField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string tGUID
        {
            get
            {
                return this.tGUIDField;
            }
            set
            {
                this.tGUIDField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class PropertyTypeUnrelatedType
    {

        private string tGUIDField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string tGUID
        {
            get
            {
                return this.tGUIDField;
            }
            set
            {
                this.tGUIDField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class PropertyTypeForm
    {

        private string fGUIDField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string fGUID
        {
            get
            {
                return this.fGUIDField;
            }
            set
            {
                this.fGUIDField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class PropertyTypeUnrelatedForm
    {

        private string fGUIDField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string fGUID
        {
            get
            {
                return this.fGUIDField;
            }
            set
            {
                this.fGUIDField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class FormType
    {

        private string formNameField;

        private FormTypeBaseForm baseFormField;

        private bool baseFormFieldSpecified;

        private List<FormTypeFormatter> formattersField;

        private SimpleDocumentationType documentationField;

        private string fGUIDField;

        private string unitsField;

        private double factorField;

        private bool factorFieldSpecified;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FormName
        {
            get
            {
                return this.formNameField;
            }
            set
            {
                this.formNameField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public FormTypeBaseForm baseForm
        {
            get
            {
                return this.baseFormField;
            }
            set
            {
                this.baseFormField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool baseFormSpecified
        {
            get
            {
                return this.baseFormFieldSpecified;
            }
            set
            {
                this.baseFormFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("formatter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public List<FormTypeFormatter> formatters
        {
            get
            {
                return this.formattersField;
            }
            set
            {
                this.formattersField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SimpleDocumentationType Documentation
        {
            get
            {
                return this.documentationField;
            }
            set
            {
                this.documentationField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string fGUID
        {
            get
            {
                return this.fGUIDField;
            }
            set
            {
                this.fGUIDField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string units
        {
            get
            {
                return this.unitsField;
            }
            set
            {
                this.unitsField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double factor
        {
            get
            {
                return this.factorField;
            }
            set
            {
                this.factorField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool factorSpecified
        {
            get
            {
                return this.factorFieldSpecified;
            }
            set
            {
                this.factorFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public enum FormTypeBaseForm
    {

        /// <remarks/>
        boolean,

        /// <remarks/>
        number,

        /// <remarks/>
        dateTime,

        /// <remarks/>
        text,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class FormTypeFormatter
    {

        private FormTypeFormatterSyntax syntaxField;

        private string valueField;

        public FormTypeFormatter()
        {
            this.syntaxField = FormTypeFormatterSyntax.iLink;
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(FormTypeFormatterSyntax.iLink)]
        public FormTypeFormatterSyntax syntax
        {
            get
            {
                return this.syntaxField;
            }
            set
            {
                this.syntaxField = value;
            }
        }

        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public enum FormTypeFormatterSyntax
    {

        /// <remarks/>
        iLink,

        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute(".net")]
        net,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class DomainType
    {

        private List<string> unitField;

        private string nameField;

        [System.Xml.Serialization.XmlElementAttribute("Unit", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<string> Unit
        {
            get
            {
                return this.unitField;
            }
            set
            {
                this.unitField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.i2group.com/Schemas/2001-12-07/LCXSchema")]
    public partial class SectorDefinition
    {

        private string sectorIDField;

        private string descriptionField;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string SectorID
        {
            get
            {
                return this.sectorIDField;
            }
            set
            {
                this.sectorIDField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GradeOne
    {

        private List<String> stringCollectionField;

        [System.Xml.Serialization.XmlArrayItemAttribute("String", IsNullable = false)]
        public List<String> StringCollection
        {
            get
            {
                return this.stringCollectionField;
            }
            set
            {
                this.stringCollectionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class String
    {

        private string idField;

        private string textField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GradeTwo
    {

        private List<String> stringCollectionField;

        [System.Xml.Serialization.XmlArrayItemAttribute("String", IsNullable = false)]
        public List<String> StringCollection
        {
            get
            {
                return this.stringCollectionField;
            }
            set
            {
                this.stringCollectionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GradeThree
    {

        private List<String> stringCollectionField;

        [System.Xml.Serialization.XmlArrayItemAttribute("String", IsNullable = false)]
        public List<String> StringCollection
        {
            get
            {
                return this.stringCollectionField;
            }
            set
            {
                this.stringCollectionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SourceHints
    {

        private List<String> stringCollectionField;

        [System.Xml.Serialization.XmlArrayItemAttribute("String", IsNullable = false)]
        public List<String> StringCollection
        {
            get
            {
                return this.stringCollectionField;
            }
            set
            {
                this.stringCollectionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Strength
    {

        private DotStyleEnum dotStyleField;

        private bool dotStyleFieldSpecified;

        private string idField;

        private string nameField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public DotStyleEnum DotStyle
        {
            get
            {
                return this.dotStyleField;
            }
            set
            {
                this.dotStyleField = value;
                DotStyleSpecified = true;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DotStyleSpecified
        {
            get
            {
                return this.dotStyleFieldSpecified;
            }
            set
            {
                this.dotStyleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum DotStyleEnum
    {

        /// <remarks/>
        DotStyleAny,

        /// <remarks/>
        DotStyleDashDot,

        /// <remarks/>
        DotStyleDashDotDot,

        /// <remarks/>
        DotStyleDashed,

        /// <remarks/>
        DotStyleDotted,

        /// <remarks/>
        DotStyleSolid,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EntityType
    {

        private int colourField;

        private string iconFileField;

        private string idField;

        private string nameField;

        private PreferredRepresentationEnum preferredRepresentationField;

        private string semanticTypeGuidField;

        private int iconShadingColourField;

        private bool iconShadingColourFieldSpecified;

        public EntityType()
        {
            this.colourField = 0;
            this.iconFileField = "General";
            this.preferredRepresentationField = PreferredRepresentationEnum.RepresentAsIcon;
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Colour
        {
            get
            {
                return this.colourField;
            }
            set
            {
                this.colourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("General")]
        public string IconFile
        {
            get
            {
                return this.iconFileField;
            }
            set
            {
                this.iconFileField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(PreferredRepresentationEnum.RepresentAsIcon)]
        public PreferredRepresentationEnum PreferredRepresentation
        {
            get
            {
                return this.preferredRepresentationField;
            }
            set
            {
                this.preferredRepresentationField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string SemanticTypeGuid
        {
            get
            {
                return this.semanticTypeGuidField;
            }
            set
            {
                this.semanticTypeGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int IconShadingColour
        {
            get
            {
                return this.iconShadingColourField;
            }
            set
            {
                this.iconShadingColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IconShadingColourSpecified
        {
            get
            {
                return this.iconShadingColourFieldSpecified;
            }
            set
            {
                this.iconShadingColourFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum PreferredRepresentationEnum
    {

        /// <remarks/>
        RepresentAsBorder,

        /// <remarks/>
        RepresentAsBox,

        /// <remarks/>
        RepresentAsCircle,

        /// <remarks/>
        RepresentAsEvent,

        /// <remarks/>
        RepresentAsIcon,

        /// <remarks/>
        RepresentAsOLE,

        /// <remarks/>
        RepresentAsTextBlock,

        /// <remarks/>
        RepresentAsTheme,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LinkType
    {

        private int colourField;

        private string idField;

        private string nameField;

        private string semanticTypeGuidField;

        public LinkType()
        {
            this.colourField = 0;
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Colour
        {
            get
            {
                return this.colourField;
            }
            set
            {
                this.colourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string SemanticTypeGuid
        {
            get
            {
                return this.semanticTypeGuidField;
            }
            set
            {
                this.semanticTypeGuidField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabaseProxy
    {

        private List<DatabaseObjectProxy> entityObjectsField;

        private List<DatabaseObjectProxy> linkObjectsField;

        private string classIDField;

        private string classNameField;

        private string connectStringField;

        private string idField;

        private string instanceNameField;

        private string displayNameField;

        private bool multipleKeysEnabledField;

        public DatabaseProxy()
        {
            this.multipleKeysEnabledField = true;
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("DatabaseObjectProxy", typeof(DatabaseObjectProxy), IsNullable = false)]
        public List<DatabaseObjectProxy> EntityObjects
        {
            get
            {
                return this.entityObjectsField;
            }
            set
            {
                this.entityObjectsField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("DatabaseObjectProxy", typeof(DatabaseObjectProxy), IsNullable = false)]
        public List<DatabaseObjectProxy> LinkObjects
        {
            get
            {
                return this.linkObjectsField;
            }
            set
            {
                this.linkObjectsField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClassID
        {
            get
            {
                return this.classIDField;
            }
            set
            {
                this.classIDField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ClassName
        {
            get
            {
                return this.classNameField;
            }
            set
            {
                this.classNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ConnectString
        {
            get
            {
                return this.connectStringField;
            }
            set
            {
                this.connectStringField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string InstanceName
        {
            get
            {
                return this.instanceNameField;
            }
            set
            {
                this.instanceNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DisplayName
        {
            get
            {
                return this.displayNameField;
            }
            set
            {
                this.displayNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool MultipleKeysEnabled
        {
            get
            {
                return this.multipleKeysEnabledField;
            }
            set
            {
                this.multipleKeysEnabledField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabaseObjectProxy
    {

        private List<DatabasePropertyType> databasePropertyTypeCollectionField;

        private string databaseProxyReferenceField;

        private string idField;

        private string nameField;

        private string parentProxyField;

        private string preferredTypeField;

        private string preferredTypeReferenceField;

        private string semanticTypeGuidField;

        [System.Xml.Serialization.XmlArrayItemAttribute("DatabasePropertyType", IsNullable = false)]
        public List<DatabasePropertyType> DatabasePropertyTypeCollection
        {
            get
            {
                return this.databasePropertyTypeCollectionField;
            }
            set
            {
                this.databasePropertyTypeCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string DatabaseProxyReference
        {
            get
            {
                return this.databaseProxyReferenceField;
            }
            set
            {
                this.databaseProxyReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ParentProxy
        {
            get
            {
                return this.parentProxyField;
            }
            set
            {
                this.parentProxyField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PreferredType
        {
            get
            {
                return this.preferredTypeField;
            }
            set
            {
                this.preferredTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string PreferredTypeReference
        {
            get
            {
                return this.preferredTypeReferenceField;
            }
            set
            {
                this.preferredTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string SemanticTypeGuid
        {
            get
            {
                return this.semanticTypeGuidField;
            }
            set
            {
                this.semanticTypeGuidField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabasePropertyType
    {

        private string dataFormGuidField;

        private AttributeTypeEnum dataTypeField;

        private bool dateSetField;

        private bool dateSetFieldSpecified;

        private string idField;

        private string nameField;

        private string semanticTypeGuidField;

        private bool timeSetField;

        private bool timeSetFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string DataFormGuid
        {
            get
            {
                return this.dataFormGuidField;
            }
            set
            {
                this.dataFormGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AttributeTypeEnum DataType
        {
            get
            {
                return this.dataTypeField;
            }
            set
            {
                this.dataTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool DateSet
        {
            get
            {
                return this.dateSetField;
            }
            set
            {
                this.dateSetField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DateSetSpecified
        {
            get
            {
                return this.dateSetFieldSpecified;
            }
            set
            {
                this.dateSetFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string SemanticTypeGuid
        {
            get
            {
                return this.semanticTypeGuidField;
            }
            set
            {
                this.semanticTypeGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool TimeSet
        {
            get
            {
                return this.timeSetField;
            }
            set
            {
                this.timeSetField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimeSetSpecified
        {
            get
            {
                return this.timeSetFieldSpecified;
            }
            set
            {
                this.timeSetFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DateTimeFormat
    {

        private string formatField;

        private string idField;

        private string nameField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Format
        {
            get
            {
                return this.formatField;
            }
            set
            {
                this.formatField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentStyleCollection
    {

        private ConnectionStyle connectionStyleField;

        private CurrentBoxStyle currentBoxStyleField;

        private CurrentCircleStyle currentCircleStyleField;

        private CurrentEventStyle currentEventStyleField;

        private CurrentIconStyle currentIconStyleField;

        private CurrentLabelStyle currentLabelStyleField;

        private CurrentLinkStyle currentLinkStyleField;

        private CurrentOleItemStyle currentOleItemStyleField;

        private CurrentTextBlockStyle currentTextBlockStyleField;

        private CurrentThemeStyle currentThemeStyleField;

        public ConnectionStyle ConnectionStyle
        {
            get
            {
                return this.connectionStyleField;
            }
            set
            {
                this.connectionStyleField = value;
            }
        }

        public CurrentBoxStyle CurrentBoxStyle
        {
            get
            {
                return this.currentBoxStyleField;
            }
            set
            {
                this.currentBoxStyleField = value;
            }
        }

        public CurrentCircleStyle CurrentCircleStyle
        {
            get
            {
                return this.currentCircleStyleField;
            }
            set
            {
                this.currentCircleStyleField = value;
            }
        }

        public CurrentEventStyle CurrentEventStyle
        {
            get
            {
                return this.currentEventStyleField;
            }
            set
            {
                this.currentEventStyleField = value;
            }
        }

        public CurrentIconStyle CurrentIconStyle
        {
            get
            {
                return this.currentIconStyleField;
            }
            set
            {
                this.currentIconStyleField = value;
            }
        }

        public CurrentLabelStyle CurrentLabelStyle
        {
            get
            {
                return this.currentLabelStyleField;
            }
            set
            {
                this.currentLabelStyleField = value;
            }
        }

        public CurrentLinkStyle CurrentLinkStyle
        {
            get
            {
                return this.currentLinkStyleField;
            }
            set
            {
                this.currentLinkStyleField = value;
            }
        }

        public CurrentOleItemStyle CurrentOleItemStyle
        {
            get
            {
                return this.currentOleItemStyleField;
            }
            set
            {
                this.currentOleItemStyleField = value;
            }
        }

        public CurrentTextBlockStyle CurrentTextBlockStyle
        {
            get
            {
                return this.currentTextBlockStyleField;
            }
            set
            {
                this.currentTextBlockStyleField = value;
            }
        }

        public CurrentThemeStyle CurrentThemeStyle
        {
            get
            {
                return this.currentThemeStyleField;
            }
            set
            {
                this.currentThemeStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ConnectionStyle
    {

        private int fanOutField;

        private bool fanOutFieldSpecified;

        private MultipleLinkStyleEnum multiplicityField;

        private bool multiplicityFieldSpecified;

        private ThemeWiringOptionEnum themeWiringField;

        private bool themeWiringFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int FanOut
        {
            get
            {
                return this.fanOutField;
            }
            set
            {
                this.fanOutField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FanOutSpecified
        {
            get
            {
                return this.fanOutFieldSpecified;
            }
            set
            {
                this.fanOutFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public MultipleLinkStyleEnum Multiplicity
        {
            get
            {
                return this.multiplicityField;
            }
            set
            {
                this.multiplicityField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MultiplicitySpecified
        {
            get
            {
                return this.multiplicityFieldSpecified;
            }
            set
            {
                this.multiplicityFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ThemeWiringOptionEnum ThemeWiring
        {
            get
            {
                return this.themeWiringField;
            }
            set
            {
                this.themeWiringField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ThemeWiringSpecified
        {
            get
            {
                return this.themeWiringFieldSpecified;
            }
            set
            {
                this.themeWiringFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum MultipleLinkStyleEnum
    {

        /// <remarks/>
        MultiplicityDirected,

        /// <remarks/>
        MultiplicityMultiple,

        /// <remarks/>
        MultiplicitySingle,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum ThemeWiringOptionEnum
    {

        /// <remarks/>
        GoesToNextEventHeight,

        /// <remarks/>
        KeepsAtEventHeight,

        /// <remarks/>
        NoDiversion,

        /// <remarks/>
        ReturnsToThemeHeight,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentBoxStyle
    {

        private CIStyle cIStyleField;

        private BoxStyle boxStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public BoxStyle BoxStyle
        {
            get
            {
                return this.boxStyleField;
            }
            set
            {
                this.boxStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CIStyle
    {

        private Font fontField;

        private List<SubItem> subItemCollectionField;

        private bool backgroundField;

        private bool backgroundFieldSpecified;

        private string dateTimeFormatField;

        private string dateTimeFormatReferenceField;

        private bool showDateTimeDescriptionField;

        private bool showDateTimeDescriptionFieldSpecified;

        private double subTextWidthField;

        private bool subTextWidthFieldSpecified;

        private bool useSubTextWidthField;

        private bool useSubTextWidthFieldSpecified;

        public Font Font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("SubItem", IsNullable = false)]
        public List<SubItem> SubItemCollection
        {
            get
            {
                return this.subItemCollectionField;
            }
            set
            {
                this.subItemCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Background
        {
            get
            {
                return this.backgroundField;
            }
            set
            {
                this.backgroundField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BackgroundSpecified
        {
            get
            {
                return this.backgroundFieldSpecified;
            }
            set
            {
                this.backgroundFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DateTimeFormat
        {
            get
            {
                return this.dateTimeFormatField;
            }
            set
            {
                this.dateTimeFormatField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string DateTimeFormatReference
        {
            get
            {
                return this.dateTimeFormatReferenceField;
            }
            set
            {
                this.dateTimeFormatReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ShowDateTimeDescription
        {
            get
            {
                return this.showDateTimeDescriptionField;
            }
            set
            {
                this.showDateTimeDescriptionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ShowDateTimeDescriptionSpecified
        {
            get
            {
                return this.showDateTimeDescriptionFieldSpecified;
            }
            set
            {
                this.showDateTimeDescriptionFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double SubTextWidth
        {
            get
            {
                return this.subTextWidthField;
            }
            set
            {
                this.subTextWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SubTextWidthSpecified
        {
            get
            {
                return this.subTextWidthFieldSpecified;
            }
            set
            {
                this.subTextWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool UseSubTextWidth
        {
            get
            {
                return this.useSubTextWidthField;
            }
            set
            {
                this.useSubTextWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool UseSubTextWidthSpecified
        {
            get
            {
                return this.useSubTextWidthFieldSpecified;
            }
            set
            {
                this.useSubTextWidthFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SubItem
    {

        private SubItemEnum typeField;

        private bool visibleField;

        public SubItem()
        {
            this.typeField = SubItemEnum.SubItemLabel;
            this.visibleField = false;
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(SubItemEnum.SubItemLabel)]
        public SubItemEnum Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum SubItemEnum
    {

        /// <remarks/>
        SubItemDateTime,

        /// <remarks/>
        SubItemDescription,

        /// <remarks/>
        SubItemGrades,

        /// <remarks/>
        SubItemLabel,

        /// <remarks/>
        SubItemPicture,

        /// <remarks/>
        SubItemPin,

        /// <remarks/>
        SubItemSourceReference,

        /// <remarks/>
        SubItemSourceType,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentCircleStyle
    {

        private CIStyle cIStyleField;

        private CircleStyle circleStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public CircleStyle CircleStyle
        {
            get
            {
                return this.circleStyleField;
            }
            set
            {
                this.circleStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CircleStyle
    {

        private bool autosizeField;

        private bool autosizeFieldSpecified;

        private int backColourField;

        private bool backColourFieldSpecified;

        private double diameterField;

        private bool diameterFieldSpecified;

        private string entityTypeReferenceField;

        private bool filledField;

        private bool filledFieldSpecified;

        private FillStyleEnum fillStyleField;

        private bool fillStyleFieldSpecified;

        private int lineWidthField;

        private bool lineWidthFieldSpecified;

        private uint lineColourField;

        private bool lineColourFieldSpecified;

        private string strengthField;

        private string strengthReferenceField;

        private string typeField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Autosize
        {
            get
            {
                return this.autosizeField;
            }
            set
            {
                this.autosizeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AutosizeSpecified
        {
            get
            {
                return this.autosizeFieldSpecified;
            }
            set
            {
                this.autosizeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BackColour
        {
            get
            {
                return this.backColourField;
            }
            set
            {
                this.backColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BackColourSpecified
        {
            get
            {
                return this.backColourFieldSpecified;
            }
            set
            {
                this.backColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Diameter
        {
            get
            {
                return this.diameterField;
            }
            set
            {
                this.diameterField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DiameterSpecified
        {
            get
            {
                return this.diameterFieldSpecified;
            }
            set
            {
                this.diameterFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Filled
        {
            get
            {
                return this.filledField;
            }
            set
            {
                this.filledField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FilledSpecified
        {
            get
            {
                return this.filledFieldSpecified;
            }
            set
            {
                this.filledFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FillStyleEnum FillStyle
        {
            get
            {
                return this.fillStyleField;
            }
            set
            {
                this.fillStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillStyleSpecified
        {
            get
            {
                return this.fillStyleFieldSpecified;
            }
            set
            {
                this.fillStyleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineWidthSpecified
        {
            get
            {
                return this.lineWidthFieldSpecified;
            }
            set
            {
                this.lineWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineColourSpecified
        {
            get
            {
                return this.lineColourFieldSpecified;
            }
            set
            {
                this.lineColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentEventStyle
    {

        private CIStyle cIStyleField;

        private EventStyle eventStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public EventStyle EventStyle
        {
            get
            {
                return this.eventStyleField;
            }
            set
            {
                this.eventStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EventStyle
    {

        private IconPicture iconPictureField;

        private TextAlignmentEnum alignmentField;

        private bool alignmentFieldSpecified;

        private AutoSizeOptionEnum autoSizeOptionField;

        private bool autoSizeOptionFieldSpecified;

        private int backColourField;

        private bool backColourFieldSpecified;

        private IconEnlargementEnum enlargementField;

        private bool enlargementFieldSpecified;

        private string entityTypeReferenceField;

        private bool filledField;

        private bool filledFieldSpecified;

        private FillStyleEnum fillStyleField;

        private bool fillStyleFieldSpecified;

        private double heightField;

        private bool heightFieldSpecified;

        private int lineWidthField;

        private bool lineWidthFieldSpecified;

        private uint lineColourField;

        private bool lineColourFieldSpecified;

        private bool linkAreaVisibleField;

        private bool linkAreaVisibleFieldSpecified;

        private string strengthField;

        private string strengthReferenceField;

        private string typeField;

        private bool typeIconVisibleField;

        private bool typeIconVisibleFieldSpecified;

        private bool typeNameVisibleField;

        private bool typeNameVisibleFieldSpecified;

        private double widthField;

        private bool widthFieldSpecified;

        private int iconShadingColourField;

        private bool iconShadingColourFieldSpecified;

        private bool overrideTypeIconField;

        private bool overrideTypeIconFieldSpecified;

        private string typeIconNameField;

        public IconPicture IconPicture
        {
            get
            {
                return this.iconPictureField;
            }
            set
            {
                this.iconPictureField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TextAlignmentEnum Alignment
        {
            get
            {
                return this.alignmentField;
            }
            set
            {
                this.alignmentField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AlignmentSpecified
        {
            get
            {
                return this.alignmentFieldSpecified;
            }
            set
            {
                this.alignmentFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AutoSizeOptionEnum AutoSizeOption
        {
            get
            {
                return this.autoSizeOptionField;
            }
            set
            {
                this.autoSizeOptionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AutoSizeOptionSpecified
        {
            get
            {
                return this.autoSizeOptionFieldSpecified;
            }
            set
            {
                this.autoSizeOptionFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BackColour
        {
            get
            {
                return this.backColourField;
            }
            set
            {
                this.backColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BackColourSpecified
        {
            get
            {
                return this.backColourFieldSpecified;
            }
            set
            {
                this.backColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public IconEnlargementEnum Enlargement
        {
            get
            {
                return this.enlargementField;
            }
            set
            {
                this.enlargementField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EnlargementSpecified
        {
            get
            {
                return this.enlargementFieldSpecified;
            }
            set
            {
                this.enlargementFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Filled
        {
            get
            {
                return this.filledField;
            }
            set
            {
                this.filledField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FilledSpecified
        {
            get
            {
                return this.filledFieldSpecified;
            }
            set
            {
                this.filledFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FillStyleEnum FillStyle
        {
            get
            {
                return this.fillStyleField;
            }
            set
            {
                this.fillStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillStyleSpecified
        {
            get
            {
                return this.fillStyleFieldSpecified;
            }
            set
            {
                this.fillStyleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HeightSpecified
        {
            get
            {
                return this.heightFieldSpecified;
            }
            set
            {
                this.heightFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineWidthSpecified
        {
            get
            {
                return this.lineWidthFieldSpecified;
            }
            set
            {
                this.lineWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineColourSpecified
        {
            get
            {
                return this.lineColourFieldSpecified;
            }
            set
            {
                this.lineColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool LinkAreaVisible
        {
            get
            {
                return this.linkAreaVisibleField;
            }
            set
            {
                this.linkAreaVisibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LinkAreaVisibleSpecified
        {
            get
            {
                return this.linkAreaVisibleFieldSpecified;
            }
            set
            {
                this.linkAreaVisibleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool TypeIconVisible
        {
            get
            {
                return this.typeIconVisibleField;
            }
            set
            {
                this.typeIconVisibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeIconVisibleSpecified
        {
            get
            {
                return this.typeIconVisibleFieldSpecified;
            }
            set
            {
                this.typeIconVisibleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool TypeNameVisible
        {
            get
            {
                return this.typeNameVisibleField;
            }
            set
            {
                this.typeNameVisibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeNameVisibleSpecified
        {
            get
            {
                return this.typeNameVisibleFieldSpecified;
            }
            set
            {
                this.typeNameVisibleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool WidthSpecified
        {
            get
            {
                return this.widthFieldSpecified;
            }
            set
            {
                this.widthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int IconShadingColour
        {
            get
            {
                return this.iconShadingColourField;
            }
            set
            {
                this.iconShadingColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IconShadingColourSpecified
        {
            get
            {
                return this.iconShadingColourFieldSpecified;
            }
            set
            {
                this.iconShadingColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool OverrideTypeIcon
        {
            get
            {
                return this.overrideTypeIconField;
            }
            set
            {
                this.overrideTypeIconField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OverrideTypeIconSpecified
        {
            get
            {
                return this.overrideTypeIconFieldSpecified;
            }
            set
            {
                this.overrideTypeIconFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TypeIconName
        {
            get
            {
                return this.typeIconNameField;
            }
            set
            {
                this.typeIconNameField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class IconPicture
    {

        private double customSizeField;

        private string dataGuidField;

        private byte[] dataField;

        private int dataLengthField;

        private bool dataLengthFieldSpecified;

        private PictureSizeMethodEnum pictureSizeMethodField;

        private bool visibleField;

        public IconPicture()
        {
            this.customSizeField = 100D;
            this.pictureSizeMethodField = PictureSizeMethodEnum.UseEnlargement;
            this.visibleField = true;
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double CustomSize
        {
            get
            {
                return this.customSizeField;
            }
            set
            {
                this.customSizeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DataGuid
        {
            get
            {
                return this.dataGuidField;
            }
            set
            {
                this.dataGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "base64Binary")]
        public byte[] Data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int DataLength
        {
            get
            {
                return this.dataLengthField;
            }
            set
            {
                this.dataLengthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DataLengthSpecified
        {
            get
            {
                return this.dataLengthFieldSpecified;
            }
            set
            {
                this.dataLengthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PictureSizeMethodEnum PictureSizeMethod
        {
            get
            {
                return this.pictureSizeMethodField;
            }
            set
            {
                this.pictureSizeMethodField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum PictureSizeMethodEnum
    {

        /// <remarks/>
        UseEnlargement,

        /// <remarks/>
        UseCustomSize,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum TextAlignmentEnum
    {

        /// <remarks/>
        TextAlignAny,

        /// <remarks/>
        TextAlignCentre,

        /// <remarks/>
        TextAlignLeft,

        /// <remarks/>
        TextAlignRight,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum AutoSizeOptionEnum
    {

        /// <remarks/>
        Autosize,

        /// <remarks/>
        AutosizeManual,

        /// <remarks/>
        AutosizeManualHeight,

        /// <remarks/>
        AutosizeManualWidth,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum IconEnlargementEnum
    {

        /// <remarks/>
        ICEnlargeDouble,

        /// <remarks/>
        ICEnlargeHalf,

        /// <remarks/>
        ICEnlargeQuadruple,

        /// <remarks/>
        ICEnlargeSingle,

        /// <remarks/>
        ICEnlargeTriple,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentIconStyle
    {

        private CIStyle cIStyleField;

        private IconStyle iconStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public IconStyle IconStyle
        {
            get
            {
                return this.iconStyleField;
            }
            set
            {
                this.iconStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class IconStyle
    {

        private FrameStyle frameStyleField;

        private IconPicture iconPictureField;

        private IconEnlargementEnum enlargementField;

        private bool enlargementFieldSpecified;

        private string entityTypeReferenceField;

        private string typeField;

        private int iconShadingColourField;

        private bool iconShadingColourFieldSpecified;

        private bool overrideTypeIconField;

        private bool overrideTypeIconFieldSpecified;

        private string typeIconNameField;

        public FrameStyle FrameStyle
        {
            get
            {
                return this.frameStyleField;
            }
            set
            {
                this.frameStyleField = value;
            }
        }

        public IconPicture IconPicture
        {
            get
            {
                return this.iconPictureField;
            }
            set
            {
                this.iconPictureField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public IconEnlargementEnum Enlargement
        {
            get
            {
                return this.enlargementField;
            }
            set
            {
                this.enlargementField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EnlargementSpecified
        {
            get
            {
                return this.enlargementFieldSpecified;
            }
            set
            {
                this.enlargementFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int IconShadingColour
        {
            get
            {
                return this.iconShadingColourField;
            }
            set
            {
                this.iconShadingColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IconShadingColourSpecified
        {
            get
            {
                return this.iconShadingColourFieldSpecified;
            }
            set
            {
                this.iconShadingColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool OverrideTypeIcon
        {
            get
            {
                return this.overrideTypeIconField;
            }
            set
            {
                this.overrideTypeIconField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OverrideTypeIconSpecified
        {
            get
            {
                return this.overrideTypeIconFieldSpecified;
            }
            set
            {
                this.overrideTypeIconFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TypeIconName
        {
            get
            {
                return this.typeIconNameField;
            }
            set
            {
                this.typeIconNameField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class FrameStyle
    {

        private int colourField;

        private bool colourFieldSpecified;

        private bool visibleField;

        private bool visibleFieldSpecified;

        private int marginField;

        private bool marginFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Colour
        {
            get
            {
                return this.colourField;
            }
            set
            {
                this.colourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColourSpecified
        {
            get
            {
                return this.colourFieldSpecified;
            }
            set
            {
                this.colourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VisibleSpecified
        {
            get
            {
                return this.visibleFieldSpecified;
            }
            set
            {
                this.visibleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Margin
        {
            get
            {
                return this.marginField;
            }
            set
            {
                this.marginField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MarginSpecified
        {
            get
            {
                return this.marginFieldSpecified;
            }
            set
            {
                this.marginFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentLabelStyle
    {

        private CIStyle cIStyleField;

        private LabelStyle labelStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public LabelStyle LabelStyle
        {
            get
            {
                return this.labelStyleField;
            }
            set
            {
                this.labelStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LabelStyle
    {

        private TextAlignmentEnum alignmentField;

        private bool alignmentFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TextAlignmentEnum Alignment
        {
            get
            {
                return this.alignmentField;
            }
            set
            {
                this.alignmentField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AlignmentSpecified
        {
            get
            {
                return this.alignmentFieldSpecified;
            }
            set
            {
                this.alignmentFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentLinkStyle
    {

        private CIStyle cIStyleField;

        private LinkStyle linkStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public LinkStyle LinkStyle
        {
            get
            {
                return this.linkStyleField;
            }
            set
            {
                this.linkStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LinkStyle
    {

        private ArrowStyleEnum arrowStyleField;

        private bool arrowStyleFieldSpecified;

        private int fanOutField;

        private bool fanOutFieldSpecified;

        private int lineWidthField;

        private bool lineWidthFieldSpecified;

        private uint lineColourField;

        private bool lineColourFieldSpecified;

        private string linkTypeReferenceField;

        private MultipleLinkStyleEnum mlStyleField;

        private bool mlStyleFieldSpecified;

        private string strengthField;

        private string strengthReferenceField;

        private string typeField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ArrowStyleEnum ArrowStyle
        {
            get
            {
                return this.arrowStyleField;
            }
            set
            {
                this.arrowStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ArrowStyleSpecified
        {
            get
            {
                return this.arrowStyleFieldSpecified;
            }
            set
            {
                this.arrowStyleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int FanOut
        {
            get
            {
                return this.fanOutField;
            }
            set
            {
                this.fanOutField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FanOutSpecified
        {
            get
            {
                return this.fanOutFieldSpecified;
            }
            set
            {
                this.fanOutFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineWidthSpecified
        {
            get
            {
                return this.lineWidthFieldSpecified;
            }
            set
            {
                this.lineWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineColourSpecified
        {
            get
            {
                return this.lineColourFieldSpecified;
            }
            set
            {
                this.lineColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string LinkTypeReference
        {
            get
            {
                return this.linkTypeReferenceField;
            }
            set
            {
                this.linkTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]

        public MultipleLinkStyleEnum MlStyle
        {
            get
            {
                return this.mlStyleField;
            }
            set
            {
                this.mlStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MlStyleSpecified
        {
            get
            {
                return this.mlStyleFieldSpecified;
            }
            set
            {
                this.mlStyleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum ArrowStyleEnum
    {

        /// <remarks/>
        ArrowNone,

        /// <remarks/> 
        ArrowOnBoth,

        /// <remarks/> 
        ArrowOnHead,

        /// <remarks/> 
        ArrowOnTail,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentOleItemStyle
    {

        private CIStyle cIStyleField;

        private OleItemStyle oleItemStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public OleItemStyle OleItemStyle
        {
            get
            {
                return this.oleItemStyleField;
            }
            set
            {
                this.oleItemStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class OleItemStyle
    {

        private string entityTypeReferenceField;

        private int lineWidthField;

        private bool lineWidthFieldSpecified;

        private uint lineColourField;

        private bool lineColourFieldSpecified;

        private bool showAsIconField;

        private bool showAsIconFieldSpecified;

        private bool showFrameField;

        private bool showFrameFieldSpecified;

        private string strengthField;

        private string strengthReferenceField;

        private string typeField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineWidthSpecified
        {
            get
            {
                return this.lineWidthFieldSpecified;
            }
            set
            {
                this.lineWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineColourSpecified
        {
            get
            {
                return this.lineColourFieldSpecified;
            }
            set
            {
                this.lineColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ShowAsIcon
        {
            get
            {
                return this.showAsIconField;
            }
            set
            {
                this.showAsIconField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ShowAsIconSpecified
        {
            get
            {
                return this.showAsIconFieldSpecified;
            }
            set
            {
                this.showAsIconFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ShowFrame
        {
            get
            {
                return this.showFrameField;
            }
            set
            {
                this.showFrameField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ShowFrameSpecified
        {
            get
            {
                return this.showFrameFieldSpecified;
            }
            set
            {
                this.showFrameFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentTextBlockStyle
    {

        private CIStyle cIStyleField;

        private TextBlockStyle textBlockStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public TextBlockStyle TextBlockStyle
        {
            get
            {
                return this.textBlockStyleField;
            }
            set
            {
                this.textBlockStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TextBlockStyle
    {

        private TextAlignmentEnum alignmentField;

        private bool alignmentFieldSpecified;

        private AutoSizeOptionEnum autoSizeOptionField;

        private bool autoSizeOptionFieldSpecified;

        private int backColourField;

        private bool backColourFieldSpecified;

        private string entityTypeReferenceField;

        private bool filledField;

        private bool filledFieldSpecified;

        private FillStyleEnum fillStyleField;

        private bool fillStyleFieldSpecified;

        private double heightField;

        private bool heightFieldSpecified;

        private int lineWidthField;

        private bool lineWidthFieldSpecified;

        private uint lineColourField;

        private bool lineColourFieldSpecified;

        private string strengthField;

        private string strengthReferenceField;

        private string typeField;

        private double widthField;

        private bool widthFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TextAlignmentEnum Alignment
        {
            get
            {
                return this.alignmentField;
            }
            set
            {
                this.alignmentField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AlignmentSpecified
        {
            get
            {
                return this.alignmentFieldSpecified;
            }
            set
            {
                this.alignmentFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public AutoSizeOptionEnum AutoSizeOption
        {
            get
            {
                return this.autoSizeOptionField;
            }
            set
            {
                this.autoSizeOptionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AutoSizeOptionSpecified
        {
            get
            {
                return this.autoSizeOptionFieldSpecified;
            }
            set
            {
                this.autoSizeOptionFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BackColour
        {
            get
            {
                return this.backColourField;
            }
            set
            {
                this.backColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BackColourSpecified
        {
            get
            {
                return this.backColourFieldSpecified;
            }
            set
            {
                this.backColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Filled
        {
            get
            {
                return this.filledField;
            }
            set
            {
                this.filledField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FilledSpecified
        {
            get
            {
                return this.filledFieldSpecified;
            }
            set
            {
                this.filledFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public FillStyleEnum FillStyle
        {
            get
            {
                return this.fillStyleField;
            }
            set
            {
                this.fillStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillStyleSpecified
        {
            get
            {
                return this.fillStyleFieldSpecified;
            }
            set
            {
                this.fillStyleFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HeightSpecified
        {
            get
            {
                return this.heightFieldSpecified;
            }
            set
            {
                this.heightFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineWidthSpecified
        {
            get
            {
                return this.lineWidthFieldSpecified;
            }
            set
            {
                this.lineWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineColourSpecified
        {
            get
            {
                return this.lineColourFieldSpecified;
            }
            set
            {
                this.lineColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool WidthSpecified
        {
            get
            {
                return this.widthFieldSpecified;
            }
            set
            {
                this.widthFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CurrentThemeStyle
    {

        private CIStyle cIStyleField;

        private ThemeStyle themeStyleField;

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public ThemeStyle ThemeStyle
        {
            get
            {
                return this.themeStyleField;
            }
            set
            {
                this.themeStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ThemeStyle
    {

        private FrameStyle frameStyleField;

        private IconPicture iconPictureField;

        private ThemeWiringOptionEnum afterThemeIconWiringField;

        private bool afterThemeIconWiringFieldSpecified;

        private IconEnlargementEnum enlargementField;

        private bool enlargementFieldSpecified;

        private string entityTypeReferenceField;

        private bool goesToChartEndField;

        private bool goesToChartEndFieldSpecified;

        private bool goesToChartStartField;

        private bool goesToChartStartFieldSpecified;

        private ThemeTerminatorEnum leftHandTerminatorField;

        private bool leftHandTerminatorFieldSpecified;

        private ThemeWiringOptionEnum leftMostWiringField;

        private bool leftMostWiringFieldSpecified;

        private int lineWidthField;

        private bool lineWidthFieldSpecified;

        private uint lineColourField;

        private bool lineColourFieldSpecified;

        private ThemeTerminatorEnum rightHandTerminatorField;

        private bool rightHandTerminatorFieldSpecified;

        private string strengthField;

        private string strengthReferenceField;

        private string typeField;

        private int iconShadingColourField;

        private bool iconShadingColourFieldSpecified;

        private bool overrideTypeIconField;

        private bool overrideTypeIconFieldSpecified;

        private string typeIconNameField;

        public FrameStyle FrameStyle
        {
            get
            {
                return this.frameStyleField;
            }
            set
            {
                this.frameStyleField = value;
            }
        }

        public IconPicture IconPicture
        {
            get
            {
                return this.iconPictureField;
            }
            set
            {
                this.iconPictureField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ThemeWiringOptionEnum AfterThemeIconWiring
        {
            get
            {
                return this.afterThemeIconWiringField;
            }
            set
            {
                this.afterThemeIconWiringField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AfterThemeIconWiringSpecified
        {
            get
            {
                return this.afterThemeIconWiringFieldSpecified;
            }
            set
            {
                this.afterThemeIconWiringFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public IconEnlargementEnum Enlargement
        {
            get
            {
                return this.enlargementField;
            }
            set
            {
                this.enlargementField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EnlargementSpecified
        {
            get
            {
                return this.enlargementFieldSpecified;
            }
            set
            {
                this.enlargementFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool GoesToChartEnd
        {
            get
            {
                return this.goesToChartEndField;
            }
            set
            {
                this.goesToChartEndField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GoesToChartEndSpecified
        {
            get
            {
                return this.goesToChartEndFieldSpecified;
            }
            set
            {
                this.goesToChartEndFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool GoesToChartStart
        {
            get
            {
                return this.goesToChartStartField;
            }
            set
            {
                this.goesToChartStartField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool GoesToChartStartSpecified
        {
            get
            {
                return this.goesToChartStartFieldSpecified;
            }
            set
            {
                this.goesToChartStartFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ThemeTerminatorEnum LeftHandTerminator
        {
            get
            {
                return this.leftHandTerminatorField;
            }
            set
            {
                this.leftHandTerminatorField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LeftHandTerminatorSpecified
        {
            get
            {
                return this.leftHandTerminatorFieldSpecified;
            }
            set
            {
                this.leftHandTerminatorFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ThemeWiringOptionEnum LeftMostWiring
        {
            get
            {
                return this.leftMostWiringField;
            }
            set
            {
                this.leftMostWiringField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LeftMostWiringSpecified
        {
            get
            {
                return this.leftMostWiringFieldSpecified;
            }
            set
            {
                this.leftMostWiringFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineWidthSpecified
        {
            get
            {
                return this.lineWidthFieldSpecified;
            }
            set
            {
                this.lineWidthFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public uint LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LineColourSpecified
        {
            get
            {
                return this.lineColourFieldSpecified;
            }
            set
            {
                this.lineColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public ThemeTerminatorEnum RightHandTerminator
        {
            get
            {
                return this.rightHandTerminatorField;
            }
            set
            {
                this.rightHandTerminatorField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RightHandTerminatorSpecified
        {
            get
            {
                return this.rightHandTerminatorFieldSpecified;
            }
            set
            {
                this.rightHandTerminatorFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int IconShadingColour
        {
            get
            {
                return this.iconShadingColourField;
            }
            set
            {
                this.iconShadingColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IconShadingColourSpecified
        {
            get
            {
                return this.iconShadingColourFieldSpecified;
            }
            set
            {
                this.iconShadingColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool OverrideTypeIcon
        {
            get
            {
                return this.overrideTypeIconField;
            }
            set
            {
                this.overrideTypeIconField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OverrideTypeIconSpecified
        {
            get
            {
                return this.overrideTypeIconFieldSpecified;
            }
            set
            {
                this.overrideTypeIconFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TypeIconName
        {
            get
            {
                return this.typeIconNameField;
            }
            set
            {
                this.typeIconNameField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum ThemeTerminatorEnum
    {

        /// <remarks/>
        TerminatorArrow,

        /// <remarks/>
        TerminatorBar,

        /// <remarks/>
        TerminatorBox,

        /// <remarks/>
        TerminatorClosedCircle,

        /// <remarks/>
        TerminatorInverseArrow,

        /// <remarks/>
        TerminatorNone,

        /// <remarks/>
        TerminatorOpenCircle,

        /// <remarks/>
        TerminatorTriangle,

        /// <remarks/>
        TerminatorY,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TimeBar
    {

        private TimeBarIntervalBandStyle timeBarIntervalBandStyleField;

        private TimeBarMarkerBandStyle timeBarMarkerBandStyleField;

        private TimeBarTickBandStyle timeBarTickBandStyleField;

        private int backColourField;

        private int borderLineColourField;

        private bool newViewTimeBarVisibleField;

        public TimeBarIntervalBandStyle TimeBarIntervalBandStyle
        {
            get
            {
                return this.timeBarIntervalBandStyleField;
            }
            set
            {
                this.timeBarIntervalBandStyleField = value;
            }
        }

        public TimeBarMarkerBandStyle TimeBarMarkerBandStyle
        {
            get
            {
                return this.timeBarMarkerBandStyleField;
            }
            set
            {
                this.timeBarMarkerBandStyleField = value;
            }
        }

        public TimeBarTickBandStyle TimeBarTickBandStyle
        {
            get
            {
                return this.timeBarTickBandStyleField;
            }
            set
            {
                this.timeBarTickBandStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BackColour
        {
            get
            {
                return this.backColourField;
            }
            set
            {
                this.backColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BorderLineColour
        {
            get
            {
                return this.borderLineColourField;
            }
            set
            {
                this.borderLineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool NewViewTimeBarVisible
        {
            get
            {
                return this.newViewTimeBarVisibleField;
            }
            set
            {
                this.newViewTimeBarVisibleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TimeBarIntervalBandStyle
    {

        private Font fontField;

        private TimeBarLabelAlignmentEnum intervalLabelAlignmentField;

        private int lineColourField;

        private bool visibleField;

        public Font Font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimeBarLabelAlignmentEnum IntervalLabelAlignment
        {
            get
            {
                return this.intervalLabelAlignmentField;
            }
            set
            {
                this.intervalLabelAlignmentField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum TimeBarLabelAlignmentEnum
    {

        /// <remarks/>
        TimeBarAlignCentre,

        /// <remarks/>
        TimeBarAlignLeft,

        /// <remarks/>
        TimeBarAlignRight,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TimeBarMarkerBandStyle
    {

        private int markerSymbolColourField;

        private int overLappingMarkerSymbolColourField;

        private bool visibleField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int MarkerSymbolColour
        {
            get
            {
                return this.markerSymbolColourField;
            }
            set
            {
                this.markerSymbolColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int OverLappingMarkerSymbolColour
        {
            get
            {
                return this.overLappingMarkerSymbolColourField;
            }
            set
            {
                this.overLappingMarkerSymbolColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TimeBarTickBandStyle
    {

        private Font fontField;

        private int lineColourField;

        private TimeBarLabelAlignmentEnum majorTickLabelAlignmentField;

        private bool visibleField;

        public Font Font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineColour
        {
            get
            {
                return this.lineColourField;
            }
            set
            {
                this.lineColourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public TimeBarLabelAlignmentEnum MajorTickLabelAlignment
        {
            get
            {
                return this.majorTickLabelAlignmentField;
            }
            set
            {
                this.majorTickLabelAlignmentField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PrintSettings
    {

        private List<Header> headerCollectionField;

        private List<Footer> footerCollectionField;

        private PageSettings pageSettingsField;

        private bool assemblePagesField;

        private bool assemblePagesFieldSpecified;

        private PrintBorderEnum borderField;

        private bool borderFieldSpecified;

        private bool centreChartField;

        private bool centreChartFieldSpecified;

        private bool colourTimeBarBackgroundField;

        private bool colourTimeBarBackgroundFieldSpecified;

        private int horzPagesField;

        private bool horzPagesFieldSpecified;

        private bool numberPagesField;

        private bool numberPagesFieldSpecified;

        private int maxExtentXField;

        private bool maxExtentXFieldSpecified;

        private int maxExtentYField;

        private bool maxExtentYFieldSpecified;

        private int minExtentXField;

        private bool minExtentXFieldSpecified;

        private int minExtentYField;

        private bool minExtentYFieldSpecified;

        private bool overlapPagesField;

        private bool overlapPagesFieldSpecified;

        private bool printChartBackgroundField;

        private bool printChartBackgroundFieldSpecified;

        private PrintOptionsEnum printThemePaneField;

        private bool printThemePaneFieldSpecified;

        private PrintOptionsEnum printTimeBarField;

        private bool printTimeBarFieldSpecified;

        private int vertPagesField;

        private bool vertPagesFieldSpecified;

        [System.Xml.Serialization.XmlArrayItemAttribute("Header", IsNullable = false)]
        public List<Header> HeaderCollection
        {
            get
            {
                return this.headerCollectionField;
            }
            set
            {
                this.headerCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Footer", IsNullable = false)]
        public List<Footer> FooterCollection
        {
            get
            {
                return this.footerCollectionField;
            }
            set
            {
                this.footerCollectionField = value;
            }
        }

        public PageSettings PageSettings
        {
            get
            {
                return this.pageSettingsField;
            }
            set
            {
                this.pageSettingsField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool AssemblePages
        {
            get
            {
                return this.assemblePagesField;
            }
            set
            {
                this.assemblePagesField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool AssemblePagesSpecified
        {
            get
            {
                return this.assemblePagesFieldSpecified;
            }
            set
            {
                this.assemblePagesFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PrintBorderEnum Border
        {
            get
            {
                return this.borderField;
            }
            set
            {
                this.borderField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BorderSpecified
        {
            get
            {
                return this.borderFieldSpecified;
            }
            set
            {
                this.borderFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool CentreChart
        {
            get
            {
                return this.centreChartField;
            }
            set
            {
                this.centreChartField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CentreChartSpecified
        {
            get
            {
                return this.centreChartFieldSpecified;
            }
            set
            {
                this.centreChartFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ColourTimeBarBackground
        {
            get
            {
                return this.colourTimeBarBackgroundField;
            }
            set
            {
                this.colourTimeBarBackgroundField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColourTimeBarBackgroundSpecified
        {
            get
            {
                return this.colourTimeBarBackgroundFieldSpecified;
            }
            set
            {
                this.colourTimeBarBackgroundFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int HorzPages
        {
            get
            {
                return this.horzPagesField;
            }
            set
            {
                this.horzPagesField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool HorzPagesSpecified
        {
            get
            {
                return this.horzPagesFieldSpecified;
            }
            set
            {
                this.horzPagesFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool NumberPages
        {
            get
            {
                return this.numberPagesField;
            }
            set
            {
                this.numberPagesField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool NumberPagesSpecified
        {
            get
            {
                return this.numberPagesFieldSpecified;
            }
            set
            {
                this.numberPagesFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int MaxExtentX
        {
            get
            {
                return this.maxExtentXField;
            }
            set
            {
                this.maxExtentXField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxExtentXSpecified
        {
            get
            {
                return this.maxExtentXFieldSpecified;
            }
            set
            {
                this.maxExtentXFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int MaxExtentY
        {
            get
            {
                return this.maxExtentYField;
            }
            set
            {
                this.maxExtentYField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MaxExtentYSpecified
        {
            get
            {
                return this.maxExtentYFieldSpecified;
            }
            set
            {
                this.maxExtentYFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int MinExtentX
        {
            get
            {
                return this.minExtentXField;
            }
            set
            {
                this.minExtentXField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinExtentXSpecified
        {
            get
            {
                return this.minExtentXFieldSpecified;
            }
            set
            {
                this.minExtentXFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int MinExtentY
        {
            get
            {
                return this.minExtentYField;
            }
            set
            {
                this.minExtentYField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool MinExtentYSpecified
        {
            get
            {
                return this.minExtentYFieldSpecified;
            }
            set
            {
                this.minExtentYFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool OverlapPages
        {
            get
            {
                return this.overlapPagesField;
            }
            set
            {
                this.overlapPagesField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OverlapPagesSpecified
        {
            get
            {
                return this.overlapPagesFieldSpecified;
            }
            set
            {
                this.overlapPagesFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool PrintChartBackground
        {
            get
            {
                return this.printChartBackgroundField;
            }
            set
            {
                this.printChartBackgroundField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PrintChartBackgroundSpecified
        {
            get
            {
                return this.printChartBackgroundFieldSpecified;
            }
            set
            {
                this.printChartBackgroundFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PrintOptionsEnum PrintThemePane
        {
            get
            {
                return this.printThemePaneField;
            }
            set
            {
                this.printThemePaneField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PrintThemePaneSpecified
        {
            get
            {
                return this.printThemePaneFieldSpecified;
            }
            set
            {
                this.printThemePaneFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PrintOptionsEnum PrintTimeBar
        {
            get
            {
                return this.printTimeBarField;
            }
            set
            {
                this.printTimeBarField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PrintTimeBarSpecified
        {
            get
            {
                return this.printTimeBarFieldSpecified;
            }
            set
            {
                this.printTimeBarFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int VertPages
        {
            get
            {
                return this.vertPagesField;
            }
            set
            {
                this.vertPagesField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VertPagesSpecified
        {
            get
            {
                return this.vertPagesFieldSpecified;
            }
            set
            {
                this.vertPagesFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Header
    {

        private HeaderFooterPositionEnum positionField;

        private bool positionFieldSpecified;

        private string propertyField;

        private bool visibleField;

        private bool visibleFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public HeaderFooterPositionEnum Position
        {
            get
            {
                return this.positionField;
            }
            set
            {
                this.positionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PositionSpecified
        {
            get
            {
                return this.positionFieldSpecified;
            }
            set
            {
                this.positionFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Property
        {
            get
            {
                return this.propertyField;
            }
            set
            {
                this.propertyField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VisibleSpecified
        {
            get
            {
                return this.visibleFieldSpecified;
            }
            set
            {
                this.visibleFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum HeaderFooterPositionEnum
    {

        /// <remarks/>
        HeaderFooterPositionLeft,

        /// <remarks/>
        HeaderFooterPositionCenter,

        /// <remarks/>
        HeaderFooterPositionRight,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Footer
    {

        private HeaderFooterPositionEnum positionField;

        private bool positionFieldSpecified;

        private string propertyField;

        private bool visibleField;

        private bool visibleFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public HeaderFooterPositionEnum Position
        {
            get
            {
                return this.positionField;
            }
            set
            {
                this.positionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PositionSpecified
        {
            get
            {
                return this.positionFieldSpecified;
            }
            set
            {
                this.positionFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Property
        {
            get
            {
                return this.propertyField;
            }
            set
            {
                this.propertyField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VisibleSpecified
        {
            get
            {
                return this.visibleFieldSpecified;
            }
            set
            {
                this.visibleFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PageSettings
    {

        private int bottomMarginField;

        private bool bottomMarginFieldSpecified;

        private int leftMarginField;

        private bool leftMarginFieldSpecified;

        private PrintOrientationEnum orientationField;

        private bool orientationFieldSpecified;

        private int paperSizeField;

        private bool paperSizeFieldSpecified;

        private string paperSizeStringField;

        private int reductionField;

        private bool reductionFieldSpecified;

        private int rightMarginField;

        private bool rightMarginFieldSpecified;

        private int topMarginField;

        private bool topMarginFieldSpecified;

        private int themeNameWidthField;

        private bool themeNameWidthFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int BottomMargin
        {
            get
            {
                return this.bottomMarginField;
            }
            set
            {
                this.bottomMarginField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool BottomMarginSpecified
        {
            get
            {
                return this.bottomMarginFieldSpecified;
            }
            set
            {
                this.bottomMarginFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LeftMargin
        {
            get
            {
                return this.leftMarginField;
            }
            set
            {
                this.leftMarginField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LeftMarginSpecified
        {
            get
            {
                return this.leftMarginFieldSpecified;
            }
            set
            {
                this.leftMarginFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public PrintOrientationEnum Orientation
        {
            get
            {
                return this.orientationField;
            }
            set
            {
                this.orientationField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrientationSpecified
        {
            get
            {
                return this.orientationFieldSpecified;
            }
            set
            {
                this.orientationFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int PaperSize
        {
            get
            {
                return this.paperSizeField;
            }
            set
            {
                this.paperSizeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool PaperSizeSpecified
        {
            get
            {
                return this.paperSizeFieldSpecified;
            }
            set
            {
                this.paperSizeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string PaperSizeString
        {
            get
            {
                return this.paperSizeStringField;
            }
            set
            {
                this.paperSizeStringField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Reduction
        {
            get
            {
                return this.reductionField;
            }
            set
            {
                this.reductionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ReductionSpecified
        {
            get
            {
                return this.reductionFieldSpecified;
            }
            set
            {
                this.reductionFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int RightMargin
        {
            get
            {
                return this.rightMarginField;
            }
            set
            {
                this.rightMarginField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RightMarginSpecified
        {
            get
            {
                return this.rightMarginFieldSpecified;
            }
            set
            {
                this.rightMarginFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int TopMargin
        {
            get
            {
                return this.topMarginField;
            }
            set
            {
                this.topMarginField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TopMarginSpecified
        {
            get
            {
                return this.topMarginFieldSpecified;
            }
            set
            {
                this.topMarginFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int ThemeNameWidth
        {
            get
            {
                return this.themeNameWidthField;
            }
            set
            {
                this.themeNameWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ThemeNameWidthSpecified
        {
            get
            {
                return this.themeNameWidthFieldSpecified;
            }
            set
            {
                this.themeNameWidthFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum PrintOrientationEnum
    {

        /// <remarks/>
        OrientLandscape,

        /// <remarks/>
        OrientPortrait,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum PrintBorderEnum
    {

        /// <remarks/>
        BorderStyleGridRef,

        /// <remarks/>
        BorderStyleLine,

        /// <remarks/>
        BorderStyleNone,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum PrintOptionsEnum
    {

        /// <remarks/>
        PrintEveryPage,

        /// <remarks/>
        PrintNone,

        /// <remarks/>
        PrintOnce,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Summary
    {

        private List<Field> fieldCollectionField;

        private List<CustomProperty> customPropertyCollectionField;

        private Origin originField;

        [System.Xml.Serialization.XmlArrayItemAttribute("Field", IsNullable = false)]
        public List<Field> FieldCollection
        {
            get
            {
                return this.fieldCollectionField;
            }
            set
            {
                this.fieldCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("CustomProperty", IsNullable = false)]
        public List<CustomProperty> CustomPropertyCollection
        {
            get
            {
                return this.customPropertyCollectionField;
            }
            set
            {
                this.customPropertyCollectionField = value;
            }
        }

        public Origin Origin
        {
            get
            {
                return this.originField;
            }
            set
            {
                this.originField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Field
    {

        private string field1Field;

        private SummaryFieldsEnum typeField;

        public Field()
        {
            this.typeField = SummaryFieldsEnum.SummaryFieldAuthor;
        }

        [System.Xml.Serialization.XmlAttributeAttribute("Field")]
        public string Field1
        {
            get
            {
                return this.field1Field;
            }
            set
            {
                this.field1Field = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(SummaryFieldsEnum.SummaryFieldAuthor)]
        public SummaryFieldsEnum Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum SummaryFieldsEnum
    {

        /// <remarks/>
        SummaryFieldApplication,

        /// <remarks/>
        SummaryFieldAuthor,

        /// <remarks/>
        SummaryFieldCategory,

        /// <remarks/>
        SummaryFieldComments,

        /// <remarks/>
        SummaryFieldKeywords,

        /// <remarks/>
        SummaryFieldSubject,

        /// <remarks/>
        SummaryFieldTemplate,

        /// <remarks/>
        SummaryFieldTitle,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CustomProperty
    {

        private string nameField;

        private CustomPropertyTypeEnum typeField;

        private bool typeFieldSpecified;

        private string valueField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public CustomPropertyTypeEnum Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TypeSpecified
        {
            get
            {
                return this.typeFieldSpecified;
            }
            set
            {
                this.typeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum CustomPropertyTypeEnum
    {

        /// <remarks/>
        Boolean,

        /// <remarks/>
        Date,

        /// <remarks/>
        Decimal,

        /// <remarks/>
        Double,

        /// <remarks/>
        Integer,

        /// <remarks/>
        String,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Origin
    {

        private System.DateTime createdDateField;

        private bool createdDateFieldSpecified;

        private int editTimeField;

        private bool editTimeFieldSpecified;

        private System.DateTime lastPrintDateField;

        private bool lastPrintDateFieldSpecified;

        private System.DateTime lastSaveDateField;

        private bool lastSaveDateFieldSpecified;

        private int revisionNumberField;

        private bool revisionNumberFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime CreatedDate
        {
            get
            {
                return this.createdDateField;
            }
            set
            {
                this.createdDateField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CreatedDateSpecified
        {
            get
            {
                return this.createdDateFieldSpecified;
            }
            set
            {
                this.createdDateFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int EditTime
        {
            get
            {
                return this.editTimeField;
            }
            set
            {
                this.editTimeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool EditTimeSpecified
        {
            get
            {
                return this.editTimeFieldSpecified;
            }
            set
            {
                this.editTimeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime LastPrintDate
        {
            get
            {
                return this.lastPrintDateField;
            }
            set
            {
                this.lastPrintDateField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastPrintDateSpecified
        {
            get
            {
                return this.lastPrintDateFieldSpecified;
            }
            set
            {
                this.lastPrintDateFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime LastSaveDate
        {
            get
            {
                return this.lastSaveDateField;
            }
            set
            {
                this.lastSaveDateField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LastSaveDateSpecified
        {
            get
            {
                return this.lastSaveDateFieldSpecified;
            }
            set
            {
                this.lastSaveDateFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int RevisionNumber
        {
            get
            {
                return this.revisionNumberField;
            }
            set
            {
                this.revisionNumberField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RevisionNumberSpecified
        {
            get
            {
                return this.revisionNumberFieldSpecified;
            }
            set
            {
                this.revisionNumberFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ChartItem
    {

        private object itemField;

        private List<Attribute> attributeCollectionField;

        private CIStyle cIStyleField;

        private PropertyBagCollection propertyBagCollectionField;

        private TimeZone timeZoneField;

        private bool dateSetField;

        private System.DateTime dateTimeField;

        private bool dateTimeFieldSpecified;

        private string dateTimeDescriptionField;

        private string descriptionField;

        private int gradeOneIndexField;

        private string gradeOneReferenceField;

        private int gradeThreeIndexField;

        private string gradeThreeReferenceField;

        private int gradeTwoIndexField;

        private string gradeTwoReferenceField;

        private string idField;

        private string groupReferenceField;

        private string labelField;

        private string localDateTimeOffsetField;

        private bool orderedField;

        private bool shownField;

        private bool shownFieldSpecified;

        private bool selectedField;

        private string sourceReferenceField;

        private string sourceTypeField;

        private bool timeSetField;

        private int xPositionField;

        public ChartItem()
        {
            this.dateSetField = false;
            this.gradeOneIndexField = 0;
            this.gradeThreeIndexField = 0;
            this.gradeTwoIndexField = 0;
            this.orderedField = false;
            this.selectedField = false;
            this.timeSetField = false;
            this.xPositionField = 0;
        }

        [System.Xml.Serialization.XmlElementAttribute("End", typeof(End))]
        [System.Xml.Serialization.XmlElementAttribute("Link", typeof(Link))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Attribute", IsNullable = false)]
        public List<Attribute> AttributeCollection
        {
            get
            {
                return this.attributeCollectionField;
            }
            set
            {
                this.attributeCollectionField = value;
            }
        }

        public CIStyle CIStyle
        {
            get
            {
                return this.cIStyleField;
            }
            set
            {
                this.cIStyleField = value;
            }
        }

        public PropertyBagCollection PropertyBagCollection
        {
            get
            {
                return this.propertyBagCollectionField;
            }
            set
            {
                this.propertyBagCollectionField = value;
            }
        }

        public TimeZone TimeZone
        {
            get
            {
                return this.timeZoneField;
            }
            set
            {
                this.timeZoneField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool DateSet
        {
            get
            {
                return this.dateSetField;
            }
            set
            {
                this.dateSetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime DateTime
        {
            get
            {
                return this.dateTimeField;
            }
            set
            {
                this.dateTimeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DateTimeSpecified
        {
            get
            {
                return this.dateTimeFieldSpecified;
            }
            set
            {
                this.dateTimeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DateTimeDescription
        {
            get
            {
                return this.dateTimeDescriptionField;
            }
            set
            {
                this.dateTimeDescriptionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int GradeOneIndex
        {
            get
            {
                return this.gradeOneIndexField;
            }
            set
            {
                this.gradeOneIndexField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GradeOneReference
        {
            get
            {
                return this.gradeOneReferenceField;
            }
            set
            {
                this.gradeOneReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int GradeThreeIndex
        {
            get
            {
                return this.gradeThreeIndexField;
            }
            set
            {
                this.gradeThreeIndexField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GradeThreeReference
        {
            get
            {
                return this.gradeThreeReferenceField;
            }
            set
            {
                this.gradeThreeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int GradeTwoIndex
        {
            get
            {
                return this.gradeTwoIndexField;
            }
            set
            {
                this.gradeTwoIndexField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GradeTwoReference
        {
            get
            {
                return this.gradeTwoReferenceField;
            }
            set
            {
                this.gradeTwoReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GroupReference
        {
            get
            {
                return this.groupReferenceField;
            }
            set
            {
                this.groupReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Label
        {
            get
            {
                return this.labelField;
            }
            set
            {
                this.labelField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string LocalDateTimeOffset
        {
            get
            {
                return this.localDateTimeOffsetField;
            }
            set
            {
                this.localDateTimeOffsetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Ordered
        {
            get
            {
                return this.orderedField;
            }
            set
            {
                this.orderedField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Shown
        {
            get
            {
                return this.shownField;
            }
            set
            {
                this.shownField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ShownSpecified
        {
            get
            {
                return this.shownFieldSpecified;
            }
            set
            {
                this.shownFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool Selected
        {
            get
            {
                return this.selectedField;
            }
            set
            {
                this.selectedField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SourceReference
        {
            get
            {
                return this.sourceReferenceField;
            }
            set
            {
                this.sourceReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SourceType
        {
            get
            {
                return this.sourceTypeField;
            }
            set
            {
                this.sourceTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool TimeSet
        {
            get
            {
                return this.timeSetField;
            }
            set
            {
                this.timeSetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int XPosition
        {
            get
            {
                return this.xPositionField;
            }
            set
            {
                this.xPositionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class End
    {

        private object itemField;

        private int xField;

        private int yField;

        private int zField;

        public End()
        {
            this.xField = 0;
            this.yField = 0;
            this.zField = 0;
        }

        [System.Xml.Serialization.XmlElementAttribute("Entity", typeof(Entity))]
        [System.Xml.Serialization.XmlElementAttribute("Label", typeof(Label))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Z
        {
            get
            {
                return this.zField;
            }
            set
            {
                this.zField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Entity
    {

        private object itemField;

        private List<DatabaseKey> databaseKeyCollectionField;

        private List<Card> cardCollectionField;

        private string entityIdField;

        private string identityField;

        private bool labelIsIdentityField;

        private string semanticTypeGuidField;

        public Entity()
        {
            this.labelIsIdentityField = false;
        }

        [System.Xml.Serialization.XmlElementAttribute("Box", typeof(Box))]
        [System.Xml.Serialization.XmlElementAttribute("Circle", typeof(Circle))]
        [System.Xml.Serialization.XmlElementAttribute("Event", typeof(Event))]
        [System.Xml.Serialization.XmlElementAttribute("Icon", typeof(Icon))]
        [System.Xml.Serialization.XmlElementAttribute("OleItem", typeof(OleItem))]
        [System.Xml.Serialization.XmlElementAttribute("TextBlock", typeof(TextBlock))]
        [System.Xml.Serialization.XmlElementAttribute("Theme", typeof(Theme))]
        public object Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("DatabaseKey", IsNullable = false)]
        public List<DatabaseKey> DatabaseKeyCollection
        {
            get
            {
                return this.databaseKeyCollectionField;
            }
            set
            {
                this.databaseKeyCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Card", IsNullable = false)]
        public List<Card> CardCollection
        {
            get
            {
                return this.cardCollectionField;
            }
            set
            {
                this.cardCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string EntityId
        {
            get
            {
                return this.entityIdField;
            }
            set
            {
                this.entityIdField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Identity
        {
            get
            {
                return this.identityField;
            }
            set
            {
                this.identityField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool LabelIsIdentity
        {
            get
            {
                return this.labelIsIdentityField;
            }
            set
            {
                this.labelIsIdentityField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string SemanticTypeGuid
        {
            get
            {
                return this.semanticTypeGuidField;
            }
            set
            {
                this.semanticTypeGuidField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Circle
    {

        private CircleStyle circleStyleField;

        public CircleStyle CircleStyle
        {
            get
            {
                return this.circleStyleField;
            }
            set
            {
                this.circleStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Event
    {

        private EventStyle eventStyleField;

        public EventStyle EventStyle
        {
            get
            {
                return this.eventStyleField;
            }
            set
            {
                this.eventStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Icon
    {

        private IconStyle iconStyleField;

        private int textXField;

        private int textYField;

        public Icon()
        {
            this.textXField = 0;
            this.textYField = 16;
        }

        public IconStyle IconStyle
        {
            get
            {
                return this.iconStyleField;
            }
            set
            {
                this.iconStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int TextX
        {
            get
            {
                return this.textXField;
            }
            set
            {
                this.textXField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(16)]
        public int TextY
        {
            get
            {
                return this.textYField;
            }
            set
            {
                this.textYField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class OleItem
    {

        private OleItemStyle oleItemStyleField;

        private string dataGuidField;

        private byte[] dataField;

        private string dataLengthField;

        private int heightField;

        private bool isCompressedField;

        private bool isLinkedField;

        private string pathField;

        private string progIDField;

        private int textXField;

        private int textYField;

        private int widthField;

        public OleItem()
        {
            this.dataLengthField = "0";
            this.heightField = 100;
            this.isCompressedField = false;
            this.isLinkedField = false;
            this.textXField = 0;
            this.textYField = 3;
            this.widthField = 100;
        }

        public OleItemStyle OleItemStyle
        {
            get
            {
                return this.oleItemStyleField;
            }
            set
            {
                this.oleItemStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DataGuid
        {
            get
            {
                return this.dataGuidField;
            }
            set
            {
                this.dataGuidField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "base64Binary")]
        public byte[] Data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "nonNegativeInteger")]
        [System.ComponentModel.DefaultValueAttribute("0")]
        public string DataLength
        {
            get
            {
                return this.dataLengthField;
            }
            set
            {
                this.dataLengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(100)]
        public int Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IsCompressed
        {
            get
            {
                return this.isCompressedField;
            }
            set
            {
                this.isCompressedField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IsLinked
        {
            get
            {
                return this.isLinkedField;
            }
            set
            {
                this.isLinkedField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Path
        {
            get
            {
                return this.pathField;
            }
            set
            {
                this.pathField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ProgID
        {
            get
            {
                return this.progIDField;
            }
            set
            {
                this.progIDField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int TextX
        {
            get
            {
                return this.textXField;
            }
            set
            {
                this.textXField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(3)]
        public int TextY
        {
            get
            {
                return this.textYField;
            }
            set
            {
                this.textYField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(100)]
        public int Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class TextBlock
    {

        private TextBlockStyle textBlockStyleField;

        public TextBlockStyle TextBlockStyle
        {
            get
            {
                return this.textBlockStyleField;
            }
            set
            {
                this.textBlockStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Theme
    {

        private ThemeStyle themeStyleField;

        private int textXField;

        private int textYField;

        public Theme()
        {
            this.textXField = 0;
            this.textYField = 16;
        }

        public ThemeStyle ThemeStyle
        {
            get
            {
                return this.themeStyleField;
            }
            set
            {
                this.themeStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int TextX
        {
            get
            {
                return this.textXField;
            }
            set
            {
                this.textXField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(16)]
        public int TextY
        {
            get
            {
                return this.textYField;
            }
            set
            {
                this.textYField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabaseKey
    {

        private List<Key> keyField;

        private List<DatabaseProperty> databasePropertyCollectionField;

        private string databaseObjectField;

        private string databaseObjectProxyReferenceField;

        private string databaseProxyClassIDField;

        private string databaseProxyInstanceNameField;

        private string databaseProxyReferenceField;

        [System.Xml.Serialization.XmlElementAttribute("Key")]
        public List<Key> Key
        {
            get
            {
                return this.keyField;
            }
            set
            {
                this.keyField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("DatabaseProperty", IsNullable = false)]
        public List<DatabaseProperty> DatabasePropertyCollection
        {
            get
            {
                return this.databasePropertyCollectionField;
            }
            set
            {
                this.databasePropertyCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DatabaseObject
        {
            get
            {
                return this.databaseObjectField;
            }
            set
            {
                this.databaseObjectField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string DatabaseObjectProxyReference
        {
            get
            {
                return this.databaseObjectProxyReferenceField;
            }
            set
            {
                this.databaseObjectProxyReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DatabaseProxyClassID
        {
            get
            {
                return this.databaseProxyClassIDField;
            }
            set
            {
                this.databaseProxyClassIDField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DatabaseProxyInstanceName
        {
            get
            {
                return this.databaseProxyInstanceNameField;
            }
            set
            {
                this.databaseProxyInstanceNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string DatabaseProxyReference
        {
            get
            {
                return this.databaseProxyReferenceField;
            }
            set
            {
                this.databaseProxyReferenceField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Key
    {

        private string itemField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabaseProperty
    {

        private string databasePropertyTypeField;

        private string databasePropertyTypeReferenceField;

        private string localDateTimeOffsetField;

        private string valueField;

        private bool visibleField;

        private bool visibleFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DatabasePropertyType
        {
            get
            {
                return this.databasePropertyTypeField;
            }
            set
            {
                this.databasePropertyTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string DatabasePropertyTypeReference
        {
            get
            {
                return this.databasePropertyTypeReferenceField;
            }
            set
            {
                this.databasePropertyTypeReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "integer")]
        public string LocalDateTimeOffset
        {
            get
            {
                return this.localDateTimeOffsetField;
            }
            set
            {
                this.localDateTimeOffsetField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VisibleSpecified
        {
            get
            {
                return this.visibleFieldSpecified;
            }
            set
            {
                this.visibleFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Label
    {

        private LabelStyle labelStyleField;

        private string labelIdField;

        public LabelStyle LabelStyle
        {
            get
            {
                return this.labelStyleField;
            }
            set
            {
                this.labelStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LabelId
        {
            get
            {
                return this.labelIdField;
            }
            set
            {
                this.labelIdField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Link
    {

        private List<DatabaseKey> databaseKeyCollectionField;

        private List<Card> cardCollectionField;

        private List<Corner> cornerCollectionField;

        private LinkStyle linkStyleField;

        private string connectionReferenceField;

        private string end1IdField;

        private string end1ReferenceField;

        private string end2IdField;

        private string end2ReferenceField;

        private int labelPosField;

        private int labelSegmentField;

        private int offsetField;

        private bool offsetFieldSpecified;

        private string semanticTypeGuidField;

        public Link()
        {
            this.labelPosField = 50;
            this.labelSegmentField = 0;
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("DatabaseKey", IsNullable = false)]
        public List<DatabaseKey> DatabaseKeyCollection
        {
            get
            {
                return this.databaseKeyCollectionField;
            }
            set
            {
                this.databaseKeyCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Card", IsNullable = false)]
        public List<Card> CardCollection
        {
            get
            {
                return this.cardCollectionField;
            }
            set
            {
                this.cardCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("Corner", IsNullable = false)]
        public List<Corner> CornerCollection
        {
            get
            {
                return this.cornerCollectionField;
            }
            set
            {
                this.cornerCollectionField = value;
            }
        }

        public LinkStyle LinkStyle
        {
            get
            {
                return this.linkStyleField;
            }
            set
            {
                this.linkStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string ConnectionReference
        {
            get
            {
                return this.connectionReferenceField;
            }
            set
            {
                this.connectionReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string End1Id
        {
            get
            {
                return this.end1IdField;
            }
            set
            {
                this.end1IdField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string End1Reference
        {
            get
            {
                return this.end1ReferenceField;
            }
            set
            {
                this.end1ReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string End2Id
        {
            get
            {
                return this.end2IdField;
            }
            set
            {
                this.end2IdField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string End2Reference
        {
            get
            {
                return this.end2ReferenceField;
            }
            set
            {
                this.end2ReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(50)]
        public int LabelPos
        {
            get
            {
                return this.labelPosField;
            }
            set
            {
                this.labelPosField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int LabelSegment
        {
            get
            {
                return this.labelSegmentField;
            }
            set
            {
                this.labelSegmentField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Offset
        {
            get
            {
                return this.offsetField;
            }
            set
            {
                this.offsetField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OffsetSpecified
        {
            get
            {
                return this.offsetFieldSpecified;
            }
            set
            {
                this.offsetFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "NCName")]
        public string SemanticTypeGuid
        {
            get
            {
                return this.semanticTypeGuidField;
            }
            set
            {
                this.semanticTypeGuidField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Corner
    {

        private string groupReferenceField;

        private int xField;

        private int yField;

        private int zField;

        public Corner()
        {
            this.xField = 0;
            this.yField = 0;
            this.zField = 0;
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string GroupReference
        {
            get
            {
                return this.groupReferenceField;
            }
            set
            {
                this.groupReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Z
        {
            get
            {
                return this.zField;
            }
            set
            {
                this.zField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Connection
    {

        private ConnectionStyle connectionStyleField;

        private string idField;

        public ConnectionStyle ConnectionStyle
        {
            get
            {
                return this.connectionStyleField;
            }
            set
            {
                this.connectionStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ThemeJunctions
    {

        private List<Junction> junctionField;

        private string themeIdField;

        private string themeReferenceField;

        [System.Xml.Serialization.XmlElementAttribute("Junction")]
        public List<Junction> Junction
        {
            get
            {
                return this.junctionField;
            }
            set
            {
                this.junctionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ThemeId
        {
            get
            {
                return this.themeIdField;
            }
            set
            {
                this.themeIdField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string ThemeReference
        {
            get
            {
                return this.themeReferenceField;
            }
            set
            {
                this.themeReferenceField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Junction
    {

        private JunctionStyle junctionStyleField;

        public JunctionStyle JunctionStyle
        {
            get
            {
                return this.junctionStyleField;
            }
            set
            {
                this.junctionStyleField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class JunctionStyle
    {

        private int colourField;

        private int lineWidthField;

        private string strengthField;

        private string strengthReferenceField;

        private JunctionOptionEnum themeColourAfterField;

        private JunctionOptionEnum themeLineWidthAfterField;

        private JunctionOptionEnum themeStrengthAfterField;

        public JunctionStyle()
        {
            this.themeColourAfterField = JunctionOptionEnum.StyleCustom;
            this.themeLineWidthAfterField = JunctionOptionEnum.StyleCustom;
            this.themeStrengthAfterField = JunctionOptionEnum.StyleCustom;
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int Colour
        {
            get
            {
                return this.colourField;
            }
            set
            {
                this.colourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string StrengthReference
        {
            get
            {
                return this.strengthReferenceField;
            }
            set
            {
                this.strengthReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(JunctionOptionEnum.StyleCustom)]
        public JunctionOptionEnum ThemeColourAfter
        {
            get
            {
                return this.themeColourAfterField;
            }
            set
            {
                this.themeColourAfterField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(JunctionOptionEnum.StyleCustom)]
        public JunctionOptionEnum ThemeLineWidthAfter
        {
            get
            {
                return this.themeLineWidthAfterField;
            }
            set
            {
                this.themeLineWidthAfterField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(JunctionOptionEnum.StyleCustom)]
        public JunctionOptionEnum ThemeStrengthAfter
        {
            get
            {
                return this.themeStrengthAfterField;
            }
            set
            {
                this.themeStrengthAfterField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum JunctionOptionEnum
    {

        /// <remarks/>
        StyleCustom,

        /// <remarks/>
        StyleFromPrevious,

        /// <remarks/>
        StyleFromTheme,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Group
    {

        private string idField;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string Id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Palette
    {

        private List<AttributeClassEntry> attributeClassEntryCollectionField;

        private List<AttributeClassEntry> attributeEntryCollectionField;

        private List<EntityTypeEntry> entityTypeEntryCollectionField;

        private List<LinkTypeEntry> linkTypeEntryCollectionField;

        private string nameField;

        private bool lockedField;

        private bool lockedFieldSpecified;

        [System.Xml.Serialization.XmlArrayItemAttribute("AttributeClassEntry", IsNullable = false)]
        public List<AttributeClassEntry> AttributeClassEntryCollection
        {
            get
            {
                return this.attributeClassEntryCollectionField;
            }
            set
            {
                this.attributeClassEntryCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("AttributeClassEntry", IsNullable = false)]
        public List<AttributeClassEntry> AttributeEntryCollection
        {
            get
            {
                return this.attributeEntryCollectionField;
            }
            set
            {
                this.attributeEntryCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("EntityTypeEntry", IsNullable = false)]
        public List<EntityTypeEntry> EntityTypeEntryCollection
        {
            get
            {
                return this.entityTypeEntryCollectionField;
            }
            set
            {
                this.entityTypeEntryCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("LinkTypeEntry", IsNullable = false)]
        public List<LinkTypeEntry> LinkTypeEntryCollection
        {
            get
            {
                return this.linkTypeEntryCollectionField;
            }
            set
            {
                this.linkTypeEntryCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Locked
        {
            get
            {
                return this.lockedField;
            }
            set
            {
                this.lockedField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool LockedSpecified
        {
            get
            {
                return this.lockedFieldSpecified;
            }
            set
            {
                this.lockedFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EntityTypeEntry
    {

        private string entityField;

        private string entityTypeReferenceField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Entity
        {
            get
            {
                return this.entityField;
            }
            set
            {
                this.entityField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string EntityTypeReference
        {
            get
            {
                return this.entityTypeReferenceField;
            }
            set
            {
                this.entityTypeReferenceField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LinkTypeEntry
    {

        private string linkTypeField;

        private string linkTypeReferenceField;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string LinkType
        {
            get
            {
                return this.linkTypeField;
            }
            set
            {
                this.linkTypeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string LinkTypeReference
        {
            get
            {
                return this.linkTypeReferenceField;
            }
            set
            {
                this.linkTypeReferenceField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PaletteBar
    {

        private string currentPaletteNameField;

        private int splitterPositionField;

        private bool splitterPositionFieldSpecified;

        private bool visibleField;

        private bool visibleFieldSpecified;

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CurrentPaletteName
        {
            get
            {
                return this.currentPaletteNameField;
            }
            set
            {
                this.currentPaletteNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int SplitterPosition
        {
            get
            {
                return this.splitterPositionField;
            }
            set
            {
                this.splitterPositionField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool SplitterPositionSpecified
        {
            get
            {
                return this.splitterPositionFieldSpecified;
            }
            set
            {
                this.splitterPositionFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Visible
        {
            get
            {
                return this.visibleField;
            }
            set
            {
                this.visibleField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool VisibleSpecified
        {
            get
            {
                return this.visibleFieldSpecified;
            }
            set
            {
                this.visibleFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LegendDefinition
    {

        private Font fontField;

        private List<LegendItem> legendItemField;

        private LegendArrangementEnum arrangeField;

        private LegendHorizontalAlignmentEnum horizontalAlignmentField;

        private bool shownField;

        private LegendVerticalAlignmentEnum verticalAlignmentField;

        private int xField;

        private int yField;

        public LegendDefinition()
        {
            this.arrangeField = LegendArrangementEnum.LegendArrangementSquare;
            this.horizontalAlignmentField = LegendHorizontalAlignmentEnum.LegendAlignmentRight;
            this.shownField = true;
            this.verticalAlignmentField = LegendVerticalAlignmentEnum.LegendAlignmentBottom;
            this.xField = 0;
            this.yField = 0;
        }

        public Font Font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [System.Xml.Serialization.XmlElementAttribute("LegendItem")]
        public List<LegendItem> LegendItem
        {
            get
            {
                return this.legendItemField;
            }
            set
            {
                this.legendItemField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(LegendArrangementEnum.LegendArrangementSquare)]
        public LegendArrangementEnum Arrange
        {
            get
            {
                return this.arrangeField;
            }
            set
            {
                this.arrangeField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(LegendHorizontalAlignmentEnum.LegendAlignmentRight)]
        public LegendHorizontalAlignmentEnum HorizontalAlignment
        {
            get
            {
                return this.horizontalAlignmentField;
            }
            set
            {
                this.horizontalAlignmentField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool Shown
        {
            get
            {
                return this.shownField;
            }
            set
            {
                this.shownField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(LegendVerticalAlignmentEnum.LegendAlignmentBottom)]
        public LegendVerticalAlignmentEnum VerticalAlignment
        {
            get
            {
                return this.verticalAlignmentField;
            }
            set
            {
                this.verticalAlignmentField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LegendItem
    {

        private Font fontField;

        private long iconShadingColourField;

        private bool iconShadingColourFieldSpecified;

        private ArrowStyleEnum arrowsField;

        private int colourField;

        private DotStyleEnum dashStyleField;

        private string imageNameField;

        private string labelField;

        private int lineWidthField;

        private LegendItemTypeEnum typeField;

        public LegendItem()
        {
            this.arrowsField = ArrowStyleEnum.ArrowNone;
            this.colourField = 0;
            this.dashStyleField = DotStyleEnum.DotStyleSolid;
            this.lineWidthField = 1;
        }

        public Font Font
        {
            get
            {
                return this.fontField;
            }
            set
            {
                this.fontField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public long IconShadingColour
        {
            get
            {
                return this.iconShadingColourField;
            }
            set
            {
                this.iconShadingColourField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IconShadingColourSpecified
        {
            get
            {
                return this.iconShadingColourFieldSpecified;
            }
            set
            {
                this.iconShadingColourFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(ArrowStyleEnum.ArrowNone)]
        public ArrowStyleEnum Arrows
        {
            get
            {
                return this.arrowsField;
            }
            set
            {
                this.arrowsField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Colour
        {
            get
            {
                return this.colourField;
            }
            set
            {
                this.colourField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(DotStyleEnum.DotStyleSolid)]
        public DotStyleEnum DashStyle
        {
            get
            {
                return this.dashStyleField;
            }
            set
            {
                this.dashStyleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ImageName
        {
            get
            {
                return this.imageNameField;
            }
            set
            {
                this.imageNameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Label
        {
            get
            {
                return this.labelField;
            }
            set
            {
                this.labelField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(1)]
        public int LineWidth
        {
            get
            {
                return this.lineWidthField;
            }
            set
            {
                this.lineWidthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public LegendItemTypeEnum Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum LegendItemTypeEnum
    {

        /// <remarks/>
        LegendItemTypeAttribute,

        /// <remarks/>
        LegendItemTypeFont,

        /// <remarks/>
        LegendItemTypeIcon,

        /// <remarks/>
        LegendItemTypeLine,

        /// <remarks/>
        LegendItemTypeLink,

        /// <remarks/>
        LegendItemTypeText,

        /// <remarks/>
        LegendItemTypeTimeZone,

        /// <remarks/>
        LegendItemTypeIconFrame,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum LegendArrangementEnum
    {

        /// <remarks/>
        LegendArrangementSquare,

        /// <remarks/>
        LegendArrangementTall,

        /// <remarks/>
        LegendArrangementWide,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum LegendHorizontalAlignmentEnum
    {

        /// <remarks/>
        LegendAlignmentFree,

        /// <remarks/>
        LegendAlignmentLeft,

        /// <remarks/>
        LegendAlignmentRight,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum LegendVerticalAlignmentEnum
    {

        /// <remarks/>
        LegendAlignmentBottom,

        /// <remarks/>
        LegendAlignmentFree,

        /// <remarks/>
        LegendAlignmentTop,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Snapshot
    {

        private List<SnapshotItem> snapshotItemCollectionField;

        private string nameField;

        private string viewClassIdField;

        private System.DateTime dateTimeField;

        private bool dateTimeFieldSpecified;

        private int xField;

        private int yField;

        private double scaleField;

        private bool showAllField;

        public Snapshot()
        {
            this.xField = 0;
            this.yField = 0;
            this.scaleField = 1D;
            this.showAllField = true;
        }

        [System.Xml.Serialization.XmlArrayItemAttribute("SnapshotItem", IsNullable = false)]
        public List<SnapshotItem> SnapshotItemCollection
        {
            get
            {
                return this.snapshotItemCollectionField;
            }
            set
            {
                this.snapshotItemCollectionField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ViewClassId
        {
            get
            {
                return this.viewClassIdField;
            }
            set
            {
                this.viewClassIdField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime DateTime
        {
            get
            {
                return this.dateTimeField;
            }
            set
            {
                this.dateTimeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DateTimeSpecified
        {
            get
            {
                return this.dateTimeFieldSpecified;
            }
            set
            {
                this.dateTimeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(1D)]
        public double Scale
        {
            get
            {
                return this.scaleField;
            }
            set
            {
                this.scaleField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(true)]
        public bool ShowAll
        {
            get
            {
                return this.showAllField;
            }
            set
            {
                this.showAllField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SnapshotItem
    {

        private string chartItemIdReferenceField;

        private int xField;

        private int yField;

        private System.DateTime dateTimeField;

        private bool dateTimeFieldSpecified;

        private bool dateSetField;

        private bool dateSetFieldSpecified;

        private bool timeSetField;

        private bool timeSetFieldSpecified;

        private bool orderedField;

        private bool orderedFieldSpecified;

        private bool shownField;

        private bool shownFieldSpecified;

        private int heightField;

        private int widthField;

        private int cornerIdField;

        private bool cornerIdFieldSpecified;

        public SnapshotItem()
        {
            this.xField = 0;
            this.yField = 0;
            this.heightField = 100;
            this.widthField = 100;
        }

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string ChartItemIdReference
        {
            get
            {
                return this.chartItemIdReferenceField;
            }
            set
            {
                this.chartItemIdReferenceField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int X
        {
            get
            {
                return this.xField;
            }
            set
            {
                this.xField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(0)]
        public int Y
        {
            get
            {
                return this.yField;
            }
            set
            {
                this.yField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime DateTime
        {
            get
            {
                return this.dateTimeField;
            }
            set
            {
                this.dateTimeField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DateTimeSpecified
        {
            get
            {
                return this.dateTimeFieldSpecified;
            }
            set
            {
                this.dateTimeFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool DateSet
        {
            get
            {
                return this.dateSetField;
            }
            set
            {
                this.dateSetField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool DateSetSpecified
        {
            get
            {
                return this.dateSetFieldSpecified;
            }
            set
            {
                this.dateSetFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool TimeSet
        {
            get
            {
                return this.timeSetField;
            }
            set
            {
                this.timeSetField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TimeSetSpecified
        {
            get
            {
                return this.timeSetFieldSpecified;
            }
            set
            {
                this.timeSetFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Ordered
        {
            get
            {
                return this.orderedField;
            }
            set
            {
                this.orderedField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool OrderedSpecified
        {
            get
            {
                return this.orderedFieldSpecified;
            }
            set
            {
                this.orderedFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool Shown
        {
            get
            {
                return this.shownField;
            }
            set
            {
                this.shownField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ShownSpecified
        {
            get
            {
                return this.shownFieldSpecified;
            }
            set
            {
                this.shownFieldSpecified = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(100)]
        public int Height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(100)]
        public int Width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int CornerId
        {
            get
            {
                return this.cornerIdField;
            }
            set
            {
                this.cornerIdField = value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CornerIdSpecified
        {
            get
            {
                return this.cornerIdFieldSpecified;
            }
            set
            {
                this.cornerIdFieldSpecified = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum LabelMergeAndPasteRuleEnum
    {

        /// <remarks/>
        LabelRuleAppend,

        /// <remarks/>
        LabelRuleDiscard,

        /// <remarks/>
        LabelRuleMerge,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum HiddenItemsVisibilityEnum
    {

        /// <remarks/>
        ItemsVisibilityHidden,

        /// <remarks/>
        ItemsVisibilityNormal,

        /// <remarks/>
        ItemsVisibilityGrayed,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public enum TypeIconDrawingModeEnum
    {

        /// <remarks/>
        Legacy,

        /// <remarks/>
        HighQuality,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ChartItemCollection
    {

        private List<ChartItem> chartItemField;

        [System.Xml.Serialization.XmlElementAttribute("ChartItem")]
        public List<ChartItem> ChartItem
        {
            get
            {
                return this.chartItemField;
            }
            set
            {
                this.chartItemField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ConnectionCollection
    {

        private List<Connection> connectionField;

        [System.Xml.Serialization.XmlElementAttribute("Connection")]
        public List<Connection> Connection
        {
            get
            {
                return this.connectionField;
            }
            set
            {
                this.connectionField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CornerCollection
    {

        private List<Corner> cornerField;

        [System.Xml.Serialization.XmlElementAttribute("Corner")]
        public List<Corner> Corner
        {
            get
            {
                return this.cornerField;
            }
            set
            {
                this.cornerField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class CustomPropertyCollection
    {

        private List<CustomProperty> customPropertyField;

        [System.Xml.Serialization.XmlElementAttribute("CustomProperty")]
        public List<CustomProperty> CustomProperty
        {
            get
            {
                return this.customPropertyField;
            }
            set
            {
                this.customPropertyField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabaseKeyCollection
    {

        private List<DatabaseKey> databaseKeyField;

        [System.Xml.Serialization.XmlElementAttribute("DatabaseKey")]
        public List<DatabaseKey> DatabaseKey
        {
            get
            {
                return this.databaseKeyField;
            }
            set
            {
                this.databaseKeyField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabasePropertyCollection
    {

        private List<DatabaseProperty> databasePropertyField;

        [System.Xml.Serialization.XmlElementAttribute("DatabaseProperty")]
        public List<DatabaseProperty> DatabaseProperty
        {
            get
            {
                return this.databasePropertyField;
            }
            set
            {
                this.databasePropertyField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabasePropertyTypeCollection
    {

        private List<DatabasePropertyType> databasePropertyTypeField;

        [System.Xml.Serialization.XmlElementAttribute("DatabasePropertyType")]
        public List<DatabasePropertyType> DatabasePropertyType
        {
            get
            {
                return this.databasePropertyTypeField;
            }
            set
            {
                this.databasePropertyTypeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DatabaseProxyCollection
    {

        private List<DatabaseProxy> databaseProxyField;

        [System.Xml.Serialization.XmlElementAttribute("DatabaseProxy")]
        public List<DatabaseProxy> DatabaseProxy
        {
            get
            {
                return this.databaseProxyField;
            }
            set
            {
                this.databaseProxyField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class DateTimeFormatCollection
    {

        private List<DateTimeFormat> dateTimeFormatField;

        [System.Xml.Serialization.XmlElementAttribute("DateTimeFormat")]
        public List<DateTimeFormat> DateTimeFormat
        {
            get
            {
                return this.dateTimeFormatField;
            }
            set
            {
                this.dateTimeFormatField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EntityObjects
    {

        private List<DatabaseObjectProxy> databaseObjectProxyField;

        [System.Xml.Serialization.XmlElementAttribute("DatabaseObjectProxy")]
        public List<DatabaseObjectProxy> DatabaseObjectProxy
        {
            get
            {
                return this.databaseObjectProxyField;
            }
            set
            {
                this.databaseObjectProxyField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EntityTypeCollection
    {

        private List<EntityType> entityTypeField;

        [System.Xml.Serialization.XmlElementAttribute("EntityType")]
        public List<EntityType> EntityType
        {
            get
            {
                return this.entityTypeField;
            }
            set
            {
                this.entityTypeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class EntityTypeEntryCollection
    {

        private List<EntityTypeEntry> entityTypeEntryField;

        [System.Xml.Serialization.XmlElementAttribute("EntityTypeEntry")]
        public List<EntityTypeEntry> EntityTypeEntry
        {
            get
            {
                return this.entityTypeEntryField;
            }
            set
            {
                this.entityTypeEntryField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class FieldCollection
    {

        private List<Field> fieldField;

        [System.Xml.Serialization.XmlElementAttribute("Field")]
        public List<Field> Field
        {
            get
            {
                return this.fieldField;
            }
            set
            {
                this.fieldField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class FooterCollection
    {

        private List<Footer> footerField;

        [System.Xml.Serialization.XmlElementAttribute("Footer")]
        public List<Footer> Footer
        {
            get
            {
                return this.footerField;
            }
            set
            {
                this.footerField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GroupCollection
    {

        private List<Group> groupField;

        [System.Xml.Serialization.XmlElementAttribute("Group")]
        public List<Group> Group
        {
            get
            {
                return this.groupField;
            }
            set
            {
                this.groupField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class HeaderCollection
    {

        private List<Header> headerField;

        [System.Xml.Serialization.XmlElementAttribute("Header")]
        public List<Header> Header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class JunctionCollection
    {

        private List<ThemeJunctions> themeJunctionsField;

        [System.Xml.Serialization.XmlElementAttribute("ThemeJunctions")]
        public List<ThemeJunctions> ThemeJunctions
        {
            get
            {
                return this.themeJunctionsField;
            }
            set
            {
                this.themeJunctionsField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LinkObjects
    {

        private List<DatabaseObjectProxy> databaseObjectProxyField;

        [System.Xml.Serialization.XmlElementAttribute("DatabaseObjectProxy")]
        public List<DatabaseObjectProxy> DatabaseObjectProxy
        {
            get
            {
                return this.databaseObjectProxyField;
            }
            set
            {
                this.databaseObjectProxyField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LinkTypeCollection
    {

        private List<LinkType> linkTypeField;

        [System.Xml.Serialization.XmlElementAttribute("LinkType")]
        public List<LinkType> LinkType
        {
            get
            {
                return this.linkTypeField;
            }
            set
            {
                this.linkTypeField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class LinkTypeEntryCollection
    {

        private List<LinkTypeEntry> linkTypeEntryField;

        [System.Xml.Serialization.XmlElementAttribute("LinkTypeEntry")]
        public List<LinkTypeEntry> LinkTypeEntry
        {
            get
            {
                return this.linkTypeEntryField;
            }
            set
            {
                this.linkTypeEntryField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PaletteBarCollection
    {

        private List<PaletteBar> paletteBarField;

        [System.Xml.Serialization.XmlElementAttribute("PaletteBar")]
        public List<PaletteBar> PaletteBar
        {
            get
            {
                return this.paletteBarField;
            }
            set
            {
                this.paletteBarField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class PaletteCollection
    {

        private List<Palette> paletteField;

        [System.Xml.Serialization.XmlElementAttribute("Palette")]
        public List<Palette> Palette
        {
            get
            {
                return this.paletteField;
            }
            set
            {
                this.paletteField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SnapshotCollection
    {

        private List<Snapshot> snapshotField;

        [System.Xml.Serialization.XmlElementAttribute("Snapshot")]
        public List<Snapshot> Snapshot
        {
            get
            {
                return this.snapshotField;
            }
            set
            {
                this.snapshotField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SnapshotItemCollection
    {

        private List<SnapshotItem> snapshotItemField;

        [System.Xml.Serialization.XmlElementAttribute("SnapshotItem")]
        public List<SnapshotItem> SnapshotItem
        {
            get
            {
                return this.snapshotItemField;
            }
            set
            {
                this.snapshotItemField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class StrengthCollection
    {

        private List<Strength> strengthField;

        [System.Xml.Serialization.XmlElementAttribute("Strength")]
        public List<Strength> Strength
        {
            get
            {
                return this.strengthField;
            }
            set
            {
                this.strengthField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class StringCollection
    {

        private List<String> stringField;

        [System.Xml.Serialization.XmlElementAttribute("String")]
        public List<String> String
        {
            get
            {
                return this.stringField;
            }
            set
            {
                this.stringField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.3761.0")]
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SubItemCollection
    {

        private List<SubItem> subItemField;

        [System.Xml.Serialization.XmlElementAttribute("SubItem")]
        public List<SubItem> SubItem
        {
            get
            {
                return this.subItemField;
            }
            set
            {
                this.subItemField = value;
            }
        }
    }
}
