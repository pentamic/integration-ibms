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
using Newtonsoft.Json.Linq;

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


            //return Ok(new Protocol
            //{
            //    protocol_data = new List<CheckIn>()
            //});
            string mess_err = "", lastSync = DateTime.Now.ToString("ddMMyyyyHHmmss");

            if (protocol.protocol_id == 20000)
            {
                var xdata = ((JArray)protocol.protocol_data.checkin_list).ToObject<List<CheckIn>>();
                
                try
                {
                    List<int> list_checkin_add = new List<int>();
                    List<int> list_checkin_update = new List<int>();
                    List<int> list_checkin_remove = new List<int>();
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

                    //List chua nhung item da xu ly, de tranh xu ly lai vao cac lan sau
                    List<int> list_card_maker_updated = new List<int>();
                    List<int> list_card_type_updated = new List<int>();
                    List<int> list_partner_group_updated = new List<int>();
                    List<int> list_partner_updated = new List<int>();
                    List<int> list_driver_updated = new List<int>();
                    List<int> list_tour_updated = new List<int>();
                    List<int> list_visit_updated = new List<int>();
                    List<int> list_location_updated = new List<int>();
                    List<int> list_payee_updated = new List<int>();
                    List<int> list_nationality_updated = new List<int>();

                    #region GetDataAPI
                    //Get Id data from API
                    foreach (var item in xdata)
                    {
                        if (item.Status)//Neu status = 1
                        {
                            //Neu ko co trong DB cho vao danh sach Insert
                            if (!list_checkin_add.Contains(item.IDs))
                                if (_context.tblCheckIns.Where(x => x.IDs == item.IDs).Count() == 0)
                                    list_checkin_add.Add(item.IDs);

                            //Neu da ton tai trong DB thi cho vao danh sach Update
                            if (!list_checkin_update.Contains(item.IDs))
                                if (_context.tblCheckIns.Where(x => x.IDs == item.IDs).Count() > 0)
                                    list_checkin_update.Add(item.IDs);
                        }
                        else //Status = 0, Update Checkin_Id co Status = 0
                        {
                            if (!list_checkin_remove.Contains(item.IDs))
                                if (_context.tblCheckIns.Where(x => x.IDs == item.IDs).Count() > 0)
                                    list_checkin_remove.Add(item.IDs);
                        }

                        if (item.card_maker != null)
                        {
                            if (!list_card_maker.Contains(item.card_maker.IDs))
                                if (_context.CardMakers.Where(x => x.IDs == item.card_maker.IDs).Count() == 0)
                                    list_card_maker.Add(item.card_maker.IDs);
                        }

                        if (item.card_type != null)
                        {
                            if (!list_card_type.Contains(item.card_type.IDs))
                                if (_context.CardTypes.Where(x => x.IDs == item.card_type.IDs).Count() == 0)
                                    list_card_type.Add(item.card_type.IDs);
                        }

                        if (item.partner_group != null)
                        {
                            if (!list_partner_group.Contains(item.partner_group.IDs))
                                if (_context.tblPartnerGroups.Where(x => x.IDs == item.partner_group.IDs).Count() == 0)
                                    list_partner_group.Add(item.partner_group.IDs);
                        }

                        if (item.partner_group.partner != null)
                        {
                            if (!list_partner.Contains(item.partner_group.partner.IDs))
                                if (_context.tblPartners.Where(x => x.IDs == item.partner_group.partner.IDs).Count() == 0)
                                    list_partner.Add(item.partner_group.partner.IDs);
                        }

                        if (item.driver != null)
                        {
                            if (!list_driver.Contains(item.driver.IDs))
                                if (_context.Drivers.Where(x => x.IDs == item.driver.IDs).Count() == 0)
                                    list_driver.Add(item.driver.IDs);
                        }

                        if (item.tour_guide != null)
                        {
                            if (!list_tour.Contains(item.tour_guide.IDs))
                                if (_context.TourGuides.Where(x => x.IDs == item.tour_guide.IDs).Count() == 0)
                                    list_tour.Add(item.tour_guide.IDs);
                        }

                        if (item.visitor_type != null)
                        {
                            if (!list_visit.Contains(item.visitor_type.IDs))
                                if (_context.VisitorTypes.Where(x => x.IDs == item.visitor_type.IDs).Count() == 0)
                                    list_visit.Add(item.visitor_type.IDs);
                        }

                        if (item.location != null)
                        {
                            if (!list_location.Contains(item.location.IDs))
                                if (_context.Locations.Where(x => x.IDs == item.location.IDs).Count() == 0)
                                    list_location.Add(item.location.IDs);
                        }

                        if (item.payee_list != null)
                        {
                            foreach (var pay in item.payee_list)
                            {
                                if (!list_payee.Contains(pay.IDs))
                                    if (_context.Payes.Where(x => x.IDs == pay.IDs).Count() == 0)
                                        list_payee.Add(pay.IDs);
                            }
                        }

                        if (item.checkin_info_list != null)
                        {
                            foreach (var nati in item.checkin_info_list)
                            {
                                if (!list_nationality.Contains(nati.NationalityId))
                                    if (_context.Nationalities.Where(x => x.IDs == nati.NationalityId).Count() == 0)
                                        list_nationality.Add(nati.NationalityId);
                            }
                        }
                    }
                    #endregion

                    if (xdata != null)
                    {
                        foreach (var item in xdata)
                        {
                            if (list_checkin_add.Contains(item.IDs))//Neu nam trong danh sach Insert
                            {

                                if (!_context.tblCheckIns.Local.Any(x => x.IDs == item.IDs))
                                {
                                    var checkin = new tblCheckIn();
                                    checkin.IDs = item.IDs;
                                    checkin.CheckInCode = item.CheckInCode;
                                    checkin.FeePort = item.FeePort;
                                    checkin.Status = item.Status;
                                    checkin.LastSync = lastSync;
                                    checkin.CreatedAt = DateTimeOffset.Now;

                                    #region CardType
                                    if (item.card_type != null)
                                    {
                                        if (item.card_type.IDs != 0)
                                        {
                                            CardType(item, list_card_type, lastSync);
                                            checkin.CardTypeId = item.card_type.IDs;
                                        }
                                    }
                                    #endregion

                                    #region CardMaker
                                    if (item.card_maker != null)
                                    {
                                        if (item.card_maker.IDs != 0)
                                        {
                                            CardMaker(item, list_card_maker, list_card_maker_updated, lastSync);
                                            checkin.CardMakerId = item.card_maker.IDs;
                                        }
                                    }
                                    #endregion

                                    #region partner_group
                                    if (item.partner_group != null)
                                    {
                                        if (item.partner_group.IDs != 0)
                                        {
                                            PartnerGroup(item, list_partner_group, lastSync);
                                        }
                                        if (item.partner_group.partner != null)
                                        {
                                            if (item.partner_group.partner.IDs != 0)
                                            {
                                                Partner(item, list_partner, lastSync);
                                                checkin.PartnerId = item.partner_group.partner.IDs;
                                            }
                                        }

                                    }
                                    #endregion

                                    #region Driver
                                    if (item.driver != null)
                                    {
                                        if (item.driver.IDs != 0)
                                        {
                                            Driver(item, list_driver, lastSync);
                                            checkin.DriverId = item.driver.IDs;
                                        }
                                    }
                                    #endregion

                                    #region Tour_guide
                                    if (item.tour_guide != null)
                                    {
                                        if (item.tour_guide.IDs != 0)
                                        {
                                            TourGuide(item, list_tour, lastSync);
                                            checkin.TourGuideId = item.tour_guide.IDs;
                                        }
                                    }
                                    #endregion

                                    #region Visitor_type
                                    if (item.visitor_type != null)
                                    {
                                        if (item.visitor_type.IDs != 0)
                                        {
                                            Visitor(item, list_visit, lastSync);
                                            checkin.VisitorTypeId = item.visitor_type.IDs;
                                        }
                                    }
                                    #endregion

                                    #region car_number_list
                                    if (item.car_number_list != null)
                                    {
                                        var del_checkin_car = _context.CheckIn_Cars.Where(x => x.CheckInId == item.IDs);
                                        if (del_checkin_car != null)
                                            _context.CheckIn_Cars.RemoveRange(del_checkin_car);

                                        foreach (var c in item.car_number_list)
                                        {
                                            var checkin_car = new CheckIn_Car();
                                            checkin_car.CheckInId = item.IDs;
                                            checkin_car.CarNumber = c.Car_Number;
                                            checkin_car.LastSync = lastSync;
                                            checkin_car.CreatedAt = DateTimeOffset.Now;

                                            _context.CheckIn_Cars.Add(checkin_car);
                                        }
                                    }
                                    #endregion

                                    #region payee_list
                                    if (item.payee_list != null)
                                    {
                                        foreach (var c in item.payee_list)
                                        {
                                            if (c.IDs != 0)
                                            {
                                                Payee(c, list_payee, lastSync);
                                            }

                                            var del_checkin_payee = _context.CheckIn_Payes.Where(x => x.CheckInId == item.IDs);
                                            if (del_checkin_payee != null)
                                                _context.CheckIn_Payes.RemoveRange(del_checkin_payee);

                                            var checkin_payee = new CheckIn_Payee();
                                            checkin_payee.CheckInId = item.IDs;
                                            checkin_payee.PayeeId = c.IDs;
                                            checkin_payee.LastSync = lastSync;
                                            checkin_payee.CreatedAt = DateTimeOffset.Now;

                                            _context.CheckIn_Payes.Add(checkin_payee);
                                        }
                                    }
                                    #endregion

                                    #region checkin_info_list
                                    if (item.checkin_info_list != null)
                                    {
                                        foreach (var i in item.checkin_info_list)
                                        {
                                            if (i.NationalityId != 0)
                                            {
                                                Nationality(i, list_nationality, lastSync);
                                            }

                                            var del_checkin_info = _context.CheckIn_Infos.Where(x => x.CheckinId == item.IDs);
                                            if (del_checkin_info != null)
                                                _context.CheckIn_Infos.RemoveRange(del_checkin_info);

                                            var checkin_info = new CheckIn_Info();
                                            checkin_info.NumberAdult = i.NumberAdult;
                                            checkin_info.NumberBaby = i.NumberBaby;
                                            checkin_info.NumberChild = i.NumberChild;
                                            if (i.NationalityId != 0)
                                                checkin_info.NationalityId = i.NationalityId;
                                            checkin_info.CheckinId = item.IDs;
                                            checkin_info.LastSync = lastSync;
                                            checkin_info.CreatedAt = DateTimeOffset.Now;

                                            _context.CheckIn_Infos.Add(checkin_info);
                                        }
                                    }
                                    #endregion

                                    #region location
                                    if (item.location != null)
                                    {
                                        if (item.location.IDs != 0)
                                        {
                                            Location(item, list_location, lastSync);
                                            checkin.LocationId = item.location.IDs;
                                        }
                                    }
                                    #endregion

                                    _context.tblCheckIns.Add(checkin);
                                }
                            }
                            else
                            {
                                if (list_checkin_remove.Contains(item.IDs))//Neu nam trong danh sach Remove, Update Status ve 0
                                {
                                    var checkin_remove = _context.tblCheckIns.Where(x => x.IDs == item.IDs).FirstOrDefault();
                                    checkin_remove.Status = item.Status;
                                    checkin_remove.LastSync = lastSync;
                                    checkin_remove.ModifiedAt = DateTimeOffset.Now;
                                }
                                else
                                {
                                    if (list_checkin_update.Contains(item.IDs))//Neu nam trong danh sach Update
                                    {
                                        var checkin_update = _context.tblCheckIns.Where(x => x.IDs == item.IDs).FirstOrDefault();
                                        checkin_update.Status = item.Status;
                                        checkin_update.CheckInCode = item.CheckInCode;
                                        checkin_update.FeePort = item.FeePort;
                                        checkin_update.LastSync = lastSync;
                                        checkin_update.ModifiedAt = DateTimeOffset.Now;

                                        #region CardType
                                        if (item.card_type != null)
                                        {
                                            if (item.card_type.IDs != 0)
                                            {
                                                CardType(item, list_card_type, lastSync);
                                                checkin_update.CardTypeId = item.card_type.IDs;
                                            }
                                        }
                                        #endregion

                                        #region CardMaker
                                        if (item.card_maker != null)
                                        {
                                            if (item.card_maker.IDs != 0)
                                            {
                                                CardMaker(item, list_card_maker, list_card_maker_updated, lastSync);
                                                checkin_update.CardMakerId = item.card_maker.IDs;
                                            }
                                        }
                                        #endregion

                                        #region partner_group
                                        if (item.partner_group != null)
                                        {
                                            if (item.partner_group.IDs != 0)
                                            {
                                                PartnerGroup(item, list_partner_group, lastSync);
                                            }
                                            if (item.partner_group.partner != null)
                                            {
                                                if (item.partner_group.partner.IDs != 0)
                                                {
                                                    Partner(item, list_partner, lastSync);
                                                    checkin_update.PartnerId = item.partner_group.partner.IDs;
                                                }
                                            }

                                        }
                                        #endregion

                                        #region driver
                                        if (item.driver != null)
                                        {
                                            if (item.driver.IDs != 0)
                                            {
                                                Driver(item, list_driver, lastSync);
                                                checkin_update.DriverId = item.driver.IDs;
                                            }
                                        }
                                        #endregion

                                        #region tour_guide
                                        if (item.tour_guide != null)
                                        {
                                            if (item.tour_guide.IDs != 0)
                                            {
                                                TourGuide(item, list_tour, lastSync);
                                                checkin_update.TourGuideId = item.tour_guide.IDs;
                                            }
                                        }
                                        #endregion

                                        #region visitor_type
                                        if (item.visitor_type != null)
                                        {
                                            if (item.visitor_type.IDs != 0)
                                            {
                                                Visitor(item, list_visit, lastSync);
                                                checkin_update.VisitorTypeId = item.visitor_type.IDs;
                                            }
                                        }
                                        #endregion

                                        #region car_number_list
                                        if (item.car_number_list != null)
                                        {
                                            var del_checkin_car = _context.CheckIn_Cars.Where(x => x.CheckInId == item.IDs);
                                            if (del_checkin_car != null)
                                                _context.CheckIn_Cars.RemoveRange(del_checkin_car);

                                            foreach (var c in item.car_number_list)
                                            {
                                                var checkin_car = new CheckIn_Car();
                                                checkin_car.CheckInId = item.IDs;
                                                checkin_car.CarNumber = c.Car_Number;
                                                checkin_car.LastSync = lastSync;
                                                checkin_car.CreatedAt = DateTimeOffset.Now;

                                                _context.CheckIn_Cars.Add(checkin_car);
                                            }
                                        }
                                        #endregion

                                        #region payee_list
                                        if (item.payee_list != null)
                                        {
                                            foreach (var c in item.payee_list)
                                            {
                                                if (c.IDs != 0)
                                                {
                                                    Payee(c, list_payee, lastSync);
                                                }

                                                var del_checkin_payee = _context.CheckIn_Payes.Where(x => x.CheckInId == item.IDs);
                                                if (del_checkin_payee != null)
                                                    _context.CheckIn_Payes.RemoveRange(del_checkin_payee);

                                                var checkin_payee = new CheckIn_Payee();
                                                checkin_payee.CheckInId = item.IDs;
                                                checkin_payee.PayeeId = c.IDs;
                                                checkin_payee.LastSync = lastSync;
                                                checkin_payee.CreatedAt = DateTimeOffset.Now;

                                                _context.CheckIn_Payes.Add(checkin_payee);
                                            }
                                        }
                                        #endregion

                                        #region checkin_info_list
                                        if (item.checkin_info_list != null)
                                        {
                                            foreach (var i in item.checkin_info_list)
                                            {
                                                if (i.NationalityId != 0)
                                                {
                                                    Nationality(i, list_nationality, lastSync);
                                                }

                                                var del_checkin_info = _context.CheckIn_Infos.Where(x => x.CheckinId == item.IDs);
                                                if (del_checkin_info != null)
                                                    _context.CheckIn_Infos.RemoveRange(del_checkin_info);

                                                var checkin_info = new CheckIn_Info();
                                                checkin_info.NumberAdult = i.NumberAdult;
                                                checkin_info.NumberBaby = i.NumberBaby;
                                                checkin_info.NumberChild = i.NumberChild;
                                                if (i.NationalityId != 0)
                                                    checkin_info.NationalityId = i.NationalityId;
                                                checkin_info.CheckinId = item.IDs;
                                                checkin_info.LastSync = lastSync;
                                                checkin_info.CreatedAt = DateTimeOffset.Now;

                                                _context.CheckIn_Infos.Add(checkin_info);
                                            }
                                        }
                                        #endregion

                                        #region location
                                        if (item.location != null)
                                        {
                                            if (item.location.IDs != 0)
                                            {
                                                Location(item, list_location, lastSync);
                                                checkin_update.LocationId = item.location.IDs;
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                        }
                        //End Foreach
                        _context.SaveChanges();
                    }
                    //end protocol.checkin_list 
                }

                catch (Exception axx)
                {
                    mess_err += axx.Message;
                }
            }
            else
            {
                if (protocol.protocol_id == 30001)
                {
                    //var xdata = ((JArray)protocol.protocol_data.checkin_list).ToObject<List<Bill>>();
                }
            }
            if (mess_err != "")
            {
                using (var context = new ApplicationDbContext())
                {
                    var sync = new DataSync();
                    sync.LastSync = lastSync;
                    sync.Status = false;
                    sync.Message = mess_err;
                    context.DataSyncs.Add(sync);
                    context.SaveChanges();
                }
                var result = new Protocol { protocol_status = false, err_code = -1, err_message = mess_err };
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
                    protocol_status = true,
                    err_code = 1,
                    err_message = "SUCCESS"
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
        private void CardType(CheckIn item, List<int> list_card_type, string lastSync)
        {
            if (list_card_type.Contains(item.card_type.IDs))//Create
            {
                var cardtype = new CardType();
                cardtype.IDs = item.card_type.IDs;
                cardtype.Name = item.card_type.Name;
                cardtype.LastSync = lastSync;
                cardtype.CreatedAt = DateTimeOffset.Now;

                _context.CardTypes.Add(cardtype);
                list_card_type.Remove(item.card_type.IDs);
            }
            else//Update
            {
                var update = _context.CardTypes.Where(x => x.IDs == item.card_type.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != item.card_type.Name)
                    {
                        update.Name = item.card_type.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void CardMaker(CheckIn item, List<int> list_card_maker, List<int> list_card_maker_updated, string lastSync)
        {
            if (list_card_maker.Contains(item.card_maker.IDs))
            {
                var cardmaker = new CardMaker();
                cardmaker.IDs = item.card_maker.IDs;
                cardmaker.Name = item.card_maker.Name;
                cardmaker.LastSync = lastSync;
                cardmaker.CreatedAt = DateTimeOffset.Now;

                _context.CardMakers.Add(cardmaker);
                list_card_maker.Remove(item.card_maker.IDs);
            }
            else//Update
            {
                var update = _context.CardMakers.Where(x => x.IDs == item.card_maker.IDs).FirstOrDefault();
                if (update != null && !list_card_maker_updated.Contains(item.card_maker.IDs))
                {
                    if (update.Name != item.card_maker.Name)
                    {
                        update.Name = item.card_maker.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;

                        list_card_maker_updated.Add(item.card_maker.IDs);
                    }
                }
            }
        }
        private void PartnerGroup(CheckIn item, List<int> list_partner_group, string lastSync)
        {
            if (list_partner_group.Contains(item.partner_group.IDs))
            {
                var partnerGroup = new tblPartnerGroup();
                partnerGroup.IDs = item.partner_group.IDs;
                partnerGroup.Name = item.partner_group.Name;
                partnerGroup.LastSync = lastSync;
                partnerGroup.CreatedAt = DateTimeOffset.Now;

                _context.tblPartnerGroups.Add(partnerGroup);
                list_partner_group.Remove(item.partner_group.IDs);
            }
            else//Update
            {
                var update = _context.tblPartnerGroups.Where(x => x.IDs == item.partner_group.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != item.partner_group.Name)
                    {
                        update.Name = item.partner_group.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void Partner(CheckIn item, List<int> list_partner, string lastSync)
        {
            if (list_partner.Contains(item.partner_group.partner.IDs))
            {
                var partner = new tblPartner();
                partner.IDs = item.partner_group.partner.IDs;
                partner.Name = item.partner_group.partner.Name;
                partner.LastSync = lastSync;
                partner.CreatedAt = DateTimeOffset.Now;

                if (item.partner_group.IDs != 0)
                    partner.PartnerGroupId = item.partner_group.IDs;

                _context.tblPartners.Add(partner);
                list_partner.Remove(item.partner_group.partner.IDs);
            }
            else//Update
            {
                var update = _context.tblPartners.Where(x => x.IDs == item.partner_group.partner.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != item.partner_group.partner.Name)
                    {
                        update.Name = item.partner_group.partner.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void Driver(CheckIn item, List<int> list_driver, string lastSync)
        {
            if (list_driver.Contains(item.driver.IDs))
            {
                var driver = new Driver();
                driver.IDs = item.driver.IDs;
                driver.Name = item.driver.Name;
                driver.LastSync = lastSync;
                driver.CreatedAt = DateTimeOffset.Now;

                _context.Drivers.Add(driver);
                list_driver.Remove(item.driver.IDs);
            }
            else//Update
            {
                var update = _context.Drivers.Where(x => x.IDs == item.driver.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != item.driver.Name)
                    {
                        update.Name = item.driver.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void TourGuide(CheckIn item, List<int> list_tour, string lastSync)
        {
            if (list_tour.Contains(item.tour_guide.IDs))
            {
                var tour = new TourGuide();
                tour.IDs = item.tour_guide.IDs;
                tour.Name = item.tour_guide.Name;
                tour.LastSync = lastSync;
                tour.CreatedAt = DateTimeOffset.Now;

                _context.TourGuides.Add(tour);
                list_tour.Remove(item.tour_guide.IDs);
            }
            else//Update
            {
                var update = _context.TourGuides.Where(x => x.IDs == item.tour_guide.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != item.tour_guide.Name)
                    {
                        update.Name = item.tour_guide.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void Visitor(CheckIn item, List<int> list_visit, string lastSync)
        {
            if (list_visit.Contains(item.visitor_type.IDs))
            {
                var visit = new VisitorType();
                visit.IDs = item.visitor_type.IDs;
                visit.Name = item.visitor_type.Name;
                visit.LastSync = lastSync;
                visit.CreatedAt = DateTimeOffset.Now;

                _context.VisitorTypes.Add(visit);
                list_visit.Remove(item.visitor_type.IDs);
            }
            else//Update
            {
                var update = _context.VisitorTypes.Where(x => x.IDs == item.visitor_type.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != item.visitor_type.Name)
                    {
                        update.Name = item.visitor_type.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void Payee(Payee c, List<int> list_payee, string lastSync)
        {
            if (list_payee.Contains(c.IDs))
            {
                var payee = new Payee();
                payee.IDs = c.IDs;
                payee.Name = c.Name;
                payee.Commission = c.Commission;
                payee.Phone = c.Phone;
                payee.LastSync = lastSync;
                payee.CreatedAt = DateTimeOffset.Now;

                _context.Payes.Add(payee);
                list_payee.Remove(c.IDs);
            }
            else//Update
            {
                var update = _context.Payes.Where(x => x.IDs == c.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != c.Name)
                    {
                        update.Name = c.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void Nationality(CheckIn_Info i, List<int> list_nationality, string lastSync)
        {
            if (list_nationality.Contains(i.NationalityId))
            {
                var national = new Nationality();
                national.IDs = i.NationalityId;
                national.Name = i.Nationality;
                national.LastSync = lastSync;
                national.CreatedAt = DateTimeOffset.Now;

                _context.Nationalities.Add(national);
                list_nationality.Remove(i.NationalityId);
            }
            else//Update
            {
                var update = _context.Nationalities.Where(x => x.IDs == i.NationalityId).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != i.Nationality)
                    {
                        update.Name = i.Nationality;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
        private void Location(CheckIn item, List<int> list_location, string lastSync)
        {
            if (list_location.Contains(item.location.IDs))
            {
                var local = new Location();
                local.IDs = item.location.IDs;
                local.BranchName = item.location.BranchName;
                local.Address = item.location.Address;
                local.LastSync = lastSync;
                local.CreatedAt = DateTimeOffset.Now;

                _context.Locations.Add(local);
                list_location.Remove(item.location.IDs);
            }
            else//Update
            {
                var update = _context.Locations.Where(x => x.IDs == item.location.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.BranchName != item.location.BranchName)
                    {
                        update.BranchName = item.location.BranchName;
                        update.Address = item.location.Address;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                    }
                }
            }
        }
    }
}
