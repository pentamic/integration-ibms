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

            //Get exists data from Database
            var list_card_maker_exists = _context.CardMakers.Select(x => x.IDs);
            var list_card_type_exists = _context.CardTypes.Select(x => x.IDs);
            var list_partner_group_exists = _context.tblPartnerGroups.Select(x => x.IDs);
            var list_partner_exists = _context.tblPartners.Select(x => x.IDs);
            var list_driver_exists = _context.Drivers.Select(x => x.IDs);
            var list_tour_exists = _context.TourGuides.Select(x => x.IDs);
            var list_visit_exists = _context.VisitorTypes.Select(x => x.IDs);
            var list_location_exists = _context.Locations.Select(x => x.IDs);
            var list_payee_exists = _context.Payes.Select(x => x.IDs);
            var list_nationality_exists = _context.Nationalities.Select(x => x.IDs);

            List<int> list_card_maker = new List<int>();
            List<int> list_card_type = new List<int>();
            List<int> list_partner_group = new List<int>();
            List<int> list_partner = new List<int>();
            List<int> list_driver = new List<int>();
            List<int> list_tour = new List<int>();
            List<int> list_visit = new List<int>();
            List<int> list_location = new List<int>();
            List<int> list_payee = new List<int>();
            List<int> list_nationality = new List<int>();

            //Get data from API
            foreach (var item in protocol.checkin_list)
            {
                if(!list_card_maker.Contains(item.card_maker.IDs))
                    list_card_maker.Add(item.card_maker.IDs);

                if (!list_card_type.Contains(item.card_type.IDs))
                    list_card_type.Add(item.card_type.IDs);

                if (!list_partner_group.Contains(item.partner_group.IDs))
                    list_partner_group.Add(item.partner_group.IDs);

                if (!list_partner.Contains(item.partner_group.partner.IDs))
                    list_partner.Add(item.partner_group.partner.IDs);

                if (!list_driver.Contains(item.driver.IDs))
                    list_driver.Add(item.driver.IDs);

                if (!list_tour.Contains(item.tour_guide.IDs))
                    list_tour.Add(item.tour_guide.IDs);

                if (!list_visit.Contains(item.visitor_type.IDs))
                    list_visit.Add(item.visitor_type.IDs);

                if (!list_location.Contains(item.location.IDs))
                    list_location.Add(item.location.IDs);

                foreach (var pay in item.payee_list)
                {
                    if (!list_payee.Contains(pay.IDs))
                        list_payee.Add(pay.IDs);
                }

                foreach (var nati in item.checkin_info_list)
                {
                    if (!list_nationality.Contains(nati.NationalityId))
                        list_nationality.Add(nati.NationalityId);
                }
            }

            //Except Data
            List<int> except_card_type = list_card_type.Except(list_card_type_exists).ToList();
            List<int> except_card_maker = list_card_maker.Except(list_card_maker_exists).ToList();

            List<int> except_partner_group = list_partner_group.Except(list_partner_group_exists).ToList();
            List<int> except_partner = list_partner.Except(list_partner_exists).ToList();

            List<int> except_driver = list_driver.Except(list_driver_exists).ToList();
            List<int> except_tour = list_tour.Except(list_tour_exists).ToList();

            List<int> except_visit = list_visit.Except(list_visit_exists).ToList();
            List<int> except_location = list_location.Except(list_location_exists).ToList();

            List<int> except_payee = list_payee.Except(list_payee_exists).ToList();
            List<int> except_nationality = list_nationality.Except(list_nationality_exists).ToList();


            string mess_err = "", id_err = "", lastSync = DateTime.Now.ToString("ddMMyyyyHHmmss");
            if (protocol.checkin_list != null)
            {
                foreach (var item in protocol.checkin_list)
                {
                    try
                    {
                        if (!_context.tblCheckIns.Local.Any(x => x.IDs == item.IDs))
                        {
                            var checkin = new tblCheckIn();
                            checkin.IDs = item.IDs;
                            checkin.CheckInCode = item.CheckInCode;
                            checkin.FeePort = item.FeePort;
                            checkin.Status = item.Status;
                            checkin.LastSync = lastSync;
                            if (item.card_type != null)
                            {
                                if (item.card_type.IDs != 0)
                                {
                                    if (except_card_type.Contains(item.card_type.IDs))
                                    {
                                        var cardtype = new CardType();
                                        cardtype.IDs = item.card_type.IDs;
                                        cardtype.Name = item.card_type.Name;
                                        cardtype.LastSync = lastSync;

                                        _context.CardTypes.Add(cardtype);
                                        except_card_type.Remove(item.card_type.IDs);
                                    }
                                    checkin.CardTypeId = item.card_type.IDs;
                                }
                            }
                            if (item.card_maker != null)
                            {
                                if (item.card_maker.IDs != 0)
                                {
                                    if (except_card_maker.Contains(item.card_maker.IDs))
                                    {
                                        var cardmaker = new CardMaker();
                                        cardmaker.IDs = item.card_maker.IDs;
                                        cardmaker.Name = item.card_maker.Name;
                                        cardmaker.LastSync = lastSync;

                                        _context.CardMakers.Add(cardmaker);
                                        except_card_maker.Remove(item.card_maker.IDs);
                                    }
                                    checkin.CardMakerId = item.card_maker.IDs;
                                }
                            }
                            if (item.partner_group != null)
                            {
                                if (item.partner_group.IDs != 0)
                                {
                                    if (except_partner_group.Contains(item.partner_group.IDs))
                                    {
                                        var partnerGroup = new tblPartnerGroup();
                                        partnerGroup.IDs = item.partner_group.IDs;
                                        partnerGroup.Name = item.partner_group.Name;
                                        partnerGroup.LastSync = lastSync;
                                        _context.tblPartnerGroups.Add(partnerGroup);
                                        except_partner_group.Remove(item.partner_group.IDs);
                                    }
                                }
                                if (item.partner_group.partner != null)
                                {
                                    if (item.partner_group.partner.IDs != 0)
                                    {
                                        if (except_partner.Contains(item.partner_group.partner.IDs))
                                        {
                                            var partner = new tblPartner();
                                            partner.IDs = item.partner_group.partner.IDs;
                                            partner.Name = item.partner_group.partner.Name;
                                            partner.LastSync = lastSync;

                                            if (item.partner_group.IDs != 0)
                                                partner.PartnerGroupId = item.partner_group.IDs;

                                            _context.tblPartners.Add(partner);
                                            except_partner.Remove(item.partner_group.partner.IDs);
                                        }
                                        checkin.PartnerId = item.partner_group.partner.IDs;
                                    }
                                }

                            }
                            if (item.driver != null)
                            {
                                if (item.driver.IDs != 0)
                                {
                                    if (except_driver.Contains(item.driver.IDs))
                                    {
                                        var driver = new Driver();
                                        driver.IDs = item.driver.IDs;
                                        driver.Name = item.driver.Name;
                                        driver.LastSync = lastSync;

                                        _context.Drivers.Add(driver);
                                        except_driver.Remove(item.driver.IDs);
                                    }
                                    checkin.DriverId = item.driver.IDs;
                                }
                            }
                            if (item.tour_guide != null)
                            {
                                if (item.tour_guide.IDs != 0)
                                {
                                    if (except_tour.Contains(item.tour_guide.IDs))
                                    {
                                        var tour = new TourGuide();
                                        tour.IDs = item.tour_guide.IDs;
                                        tour.Name = item.tour_guide.Name;
                                        tour.LastSync = lastSync;

                                        _context.TourGuides.Add(tour);
                                        except_tour.Remove(item.tour_guide.IDs);
                                    }
                                    checkin.TourGuideId = item.tour_guide.IDs;
                                }
                            }
                            if (item.visitor_type != null)
                            {
                                if (item.visitor_type.IDs != 0)
                                {
                                    if (except_visit.Contains(item.visitor_type.IDs))
                                    {
                                        var visit = new VisitorType();
                                        visit.IDs = item.visitor_type.IDs;
                                        visit.Name = item.visitor_type.Name;
                                        visit.LastSync = lastSync;

                                        _context.VisitorTypes.Add(visit);
                                        except_visit.Remove(item.visitor_type.IDs);
                                    }
                                    checkin.VisitorTypeId = item.visitor_type.IDs;
                                }
                            }
                            if (item.car_number_list != null)
                            {
                                foreach (var c in item.car_number_list)
                                {
                                    var checkin_car = new CheckIn_Car();
                                    checkin_car.CheckInId = item.IDs;
                                    checkin_car.CarNumber = c.Car_Number;
                                    checkin_car.LastSync = lastSync;

                                    _context.CheckIn_Cars.Add(checkin_car);
                                }
                            }
                            if (item.payee_list != null)
                            {
                                foreach (var c in item.payee_list)
                                {
                                    if (c.IDs != 0)
                                    {
                                        if (except_payee.Contains(c.IDs))
                                        {
                                            var payee = new Payee();
                                            payee.IDs = c.IDs;
                                            payee.Name = c.Name;
                                            payee.Commission = c.Commission;
                                            payee.Phone = c.Phone;
                                            payee.LastSync = lastSync;

                                            _context.Payes.Add(payee);
                                            except_payee.Remove(c.IDs);
                                        }
                                    }

                                    var checkin_payee = new CheckIn_Payee();
                                    checkin_payee.CheckInId = item.IDs;
                                    checkin_payee.PayeeId = c.IDs;
                                    checkin_payee.LastSync = lastSync;

                                    _context.CheckIn_Payes.Add(checkin_payee);
                                }
                            }
                            if (item.checkin_info_list != null)
                            {
                                foreach (var i in item.checkin_info_list)
                                {
                                    if (i.NationalityId != 0)
                                    {
                                        if (except_nationality.Contains(i.NationalityId))
                                        {
                                            var national = new Nationality();
                                            national.IDs = i.NationalityId;
                                            national.Name = i.Nationality;
                                            national.LastSync = lastSync;

                                            _context.Nationalities.Add(national);
                                            except_nationality.Remove(i.NationalityId);
                                        }
                                    }

                                    var checkin_info = new CheckIn_Info();
                                    checkin_info.NumberAdult = i.NumberAdult;
                                    checkin_info.NumberBaby = i.NumberBaby;
                                    checkin_info.NumberChild = i.NumberChild;
                                    if (i.NationalityId != 0)
                                        checkin_info.NationalityId = i.NationalityId;
                                    checkin_info.CheckinId = item.IDs;
                                    checkin_info.LastSync = lastSync;

                                    _context.CheckIn_Infos.Add(checkin_info);
                                }
                            }
                            if (item.location != null)
                            {
                                if (item.location.IDs != 0)
                                {
                                    if (except_location.Contains(item.location.IDs))
                                    {
                                        var local = new Location();
                                        local.IDs = item.location.IDs;
                                        local.BranchName = item.location.BranchName;
                                        local.Address = item.location.Address;
                                        local.LastSync = lastSync;

                                        _context.Locations.Add(local);
                                        except_location.Remove(item.location.IDs);
                                    }
                                    checkin.LocationId = item.location.IDs;
                                }
                            }
                            _context.tblCheckIns.Add(checkin);
                        }
                    }
                    catch (Exception ax)
                    {

                        mess_err += ax.Message + " ; \n";
                        id_err += item.IDs.ToString() + " ; \n";
                        continue;
                    }
                }
                _context.SaveChanges();
            }
            if (id_err != "")
            {
                var sync = new DataSync();
                sync.LastSync = lastSync;
                sync.Status = false;
                sync.Message = id_err;
                _context.DataSyncs.Add(sync);
                _context.SaveChanges();

                var result = new Protocol { protocol_status = false, err_message = mess_err };
                return Ok(result);
            }
            else
            {
                var sync = new DataSync();
                sync.LastSync = lastSync;
                sync.Status = true;
                _context.DataSyncs.Add(sync);
                _context.SaveChanges();

                var result = protocol ?? new Protocol
                {
                    protocol_status = true
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
