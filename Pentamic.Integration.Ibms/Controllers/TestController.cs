using Newtonsoft.Json;
using Pentamic.Integration.Ibms.Models;
using Serilog;
using SerilogWeb.Classic.Enrichers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Web;
using System.Web.Http;
using Pentamic.Integration.Ibms.Helpers;

namespace Pentamic.Integration.Ibms.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : ApiController
    {
        private ApplicationDbContext _context;
        public TestController()
        {
            _context = new ApplicationDbContext();
            string path = HttpContext.Current.Server.MapPath("~/logs");
            Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose().Enrich.With<HttpRequestIdEnricher>().WriteTo.RollingFile(path + "/log-{Date}.txt", shared: true).CreateLogger();
        }

        [HttpPost]
        [Route("string")]
        public string String([FromBody]string value)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received string: {value}", value);
            return $"String received: {value}";
        }

        [HttpPost]
        [Route("base64")]
        public Protocol Base64([FromBody]string value)
        {
            Log.Information("Request: ,{Content-Type}, {Charset}", Request.Content.Headers.ContentType.MediaType, Request.Content.Headers.ContentType.CharSet);
            if (value == null)
            {
                Log.Warning("Received null string");
                return new Protocol
                {
                    protocol_id = 0,
                    branch_id = 0,
                    protocol_status = false,
                    partner_name = "",
                    partner_pass = "",
                    err_code = 0,
                    err_message = "Input string is null"
                };
            }
            try
            {
                Log.Information("Received base64 string {value}", value);
                var decodedData = Convert.FromBase64String(value);
                var jsonString = Encoding.UTF8.GetString(decodedData);
                Log.Information("Decoded json string from base64 {0}", jsonString);
                var model = JsonConvert.DeserializeObject<Protocol>(jsonString);
                return new Protocol
                {
                    protocol_id = model.protocol_id,
                    branch_id = model.branch_id,
                    protocol_status = true,
                    partner_name = "",
                    partner_pass = "",
                    err_code = 0,
                    err_message = null
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error get package value");
                return new Protocol
                {
                    protocol_id = 0,
                    branch_id = 0,
                    protocol_status = false,
                    partner_name = "",
                    partner_pass = "",
                    err_code = 0,
                    err_message = e.Message
                };
            }
        }


        [HttpPost]
        [Route("json")]
        public IHttpActionResult Json(Protocol protocol)
        {
            //Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            //Log.Information("Received package: {Package}", JsonConvert.SerializeObject(protocol));

            //var checkin_id_list = from x in _context.CheckIns
            //                      select new { x.id };
            string err = "";
            //if (protocol.checkin_list != null)
            //{

            //    foreach (var item in protocol.checkin_list)
            //    {
            //        try
            //        {
            //            var checkin = new CheckIn();
            //            checkin.id = item.id;
            //            checkin.checkin_code = item.checkin_code;
            //            checkin.fee_port = item.fee_port;
            //            checkin.status = item.status;

            //            if (item.card_type != null)
            //            {
            //                if (item.card_type.id != 0)
            //                {
            //                    var cardtype = new CardType();
            //                    cardtype.id = item.card_type.id;
            //                    cardtype.card_name = item.card_type.card_name;
            //                    //checkin.card_type_id = item.card_type.id;
            //                    _context.CardTypes.Add(cardtype);
            //                }
            //            }
            //            if (item.card_maker != null)
            //            {
            //                if (item.card_maker.id != 0)
            //                {
            //                    var cardmaker = new CardMaker();
            //                    cardmaker.id = item.card_maker.id;
            //                    cardmaker.card_maker_name = item.card_maker.card_maker_name;
            //                    //checkin.card_maker_id = item.card_maker.id;

            //                    _context.CardMakers.Add(cardmaker);
            //                }
            //            }
            //            if (item.partner_group != null)
            //            {
            //                if (item.partner_group.id != 0)
            //                {
            //                    var partnerGroup = new PartnerGroup();
            //                    partnerGroup.id = item.partner_group.id;
            //                    partnerGroup.fullName = item.partner_group.fullName;

            //                    _context.PartnerGroups.Add(partnerGroup);
            //                }
            //                if (item.partner_group.partner != null)
            //                {
            //                    if (item.partner_group.partner.id != 0)
            //                    {
            //                        var partner = new Partner();
            //                        partner.id = item.partner_group.partner.id;
            //                        partner.fullName = item.partner_group.partner.fullName;
            //                        if (item.partner_group.id != 0)
            //                            partner.partner_group_id = item.partner_group.id;

            //                        _context.Partners.Add(partner);
            //                    }
            //                }

            //            }
            //            if (item.driver != null)
            //            {
            //                if (item.driver.id != 0)
            //                {
            //                    var driver = new Driver();
            //                    driver.id = item.driver.id;
            //                    driver.fullName = item.driver.fullName;
            //                    //checkin.driver_id = item.driver.id;

            //                    _context.Drivers.Add(driver);
            //                }
            //            }
            //            if (item.tour_guide != null)
            //            {
            //                if (item.tour_guide.id != 0)
            //                {
            //                    var tour = new TourGuide();
            //                    tour.id = item.tour_guide.id;
            //                    tour.fullName = item.tour_guide.fullName;
            //                    //checkin.tour_guide_id = item.tour_guide.id;

            //                    _context.TourGuides.Add(tour);
            //                }
            //            }
            //            if (item.visitor_type != null)
            //            {
            //                if (item.visitor_type.id != 0)
            //                {
            //                    var visit = new VisitorType();
            //                    visit.id = item.visitor_type.id;
            //                    visit.visitor_type_name = item.visitor_type.visitor_type_name;
            //                    //checkin.visitor_type_id = item.visitor_type.id;

            //                    _context.VisitorTypes.Add(visit);
            //                }
            //            }
            //            //if (item.car_number_list != null)
            //            //{
            //            //    foreach (var c in item.car_number_list)
            //            //    {
            //            //        var car = new CarNumber();
            //            //        car.car_number = c.car_number;

            //            //        _context.CarNumbers.Add(car);

            //            //        var checkin_car = new CheckIn_Car();
            //            //        checkin_car.CheckInId  = item.id;
            //            //        checkin_car.CarNumber = c.car_number;

            //            //        _context.CheckIn_Cars.Add(checkin_car);
            //            //    }
            //            //}
            //            if (item.payee_list != null)
            //            {
            //                foreach (var c in item.payee_list)
            //                {
            //                    if (c.id != 0)
            //                    {
            //                        var payee = new Payee();
            //                        payee.id = c.id;
            //                        payee.payee_name = c.payee_name;
            //                        payee.payee_commission = c.payee_commission;
            //                        payee.payee_phone = c.payee_phone;

            //                        _context.Payes.Add(payee);
            //                    }

            //                    var checkin_payee = new CheckIn_Payee();
            //                    checkin_payee.CheckInId = item.id;
            //                    checkin_payee.PayeeId = c.id;

            //                    _context.CheckIn_Payes.Add(checkin_payee);
            //                }
            //            }
            //            if (item.checkin_info_list != null)
            //            {
            //                foreach (var i in item.checkin_info_list)
            //                {
            //                    if (i.nationality_id != 0)
            //                    {
            //                        var national = new Nationality();
            //                        national.id = i.nationality_id;
            //                        national.nationality_name = i.nationality;

            //                        _context.Nationalities.Add(national);
            //                    }

            //                    var checkin_info = new CheckIn_Info();
            //                    checkin_info.number_adult = i.number_adult;
            //                    checkin_info.number_baby = i.number_baby;
            //                    checkin_info.number_child = i.number_child;
            //                    checkin_info.nationality_id = i.nationality_id;
            //                    checkin_info.checkin_id = item.id;

            //                    _context.CheckIn_Infos.Add(checkin_info);
            //                }
            //            }
            //            if (item.location != null)
            //            {
            //                if (item.location.id != 0)
            //                {
            //                    var local = new Location();
            //                    local.id = item.location.id;
            //                    local.branchName = item.location.branchName;
            //                    local.address = item.location.address;
            //                    //checkin.location_id = item.location.id;

            //                    _context.Locations.Add(local);
            //                }
            //            }
            //            _context.CheckIns.Add(checkin);
            //            _context.SaveChanges();
            //        }
            //        catch (Exception ax)
            //        {

            //            err += item.id + " ; ";
            //            break;
            //        }
            //    }

            //}
            if (err != "")
            {
                var result = new Protocol { protocol_status = false, err_message = err };
                return Ok(result);
            }
            else
            {
                var result = protocol ?? new Protocol
                {
                    protocol_status = false,
                    err_message = err
                };
                return Ok(result);
            }
        }

        [HttpPost]
        [Route("json2")]
        public HttpResponseMessage Json2(Protocol package)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(package));
            var result = package ?? new Protocol
            {
                protocol_status = false
            };
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(result), Encoding.UTF8, "application/json"),
            };
        }

        [HttpPost]
        [Route("json3")]
        public Protocol Json3([FromBody]Protocol package)
        {
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(package));
            return package ?? new Protocol
            {
                protocol_status = false
            };
        }
    }
}
