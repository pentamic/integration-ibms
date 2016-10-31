﻿using Newtonsoft.Json;
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
            Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
            Log.Information("Received package: {Package}", JsonConvert.SerializeObject(protocol));

            //return Ok(new Protocol
            //{
            //    protocol_data = new List<CheckIn>()
            //});
            string mess_err = "", lastSync = DateTime.Now.ToString("ddMMyyyyHHmmss");

            
            if (protocol.protocol_id == 20000)//CheckIn
            {
                #region CheckIn
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
                                if (_context.Branchs.Where(x => x.IDs == item.location.IDs).Count() == 0)
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

                                    #region Branch
                                    if (item.location != null)
                                    {
                                        if (item.location.IDs != 0)
                                        {
                                            Branch(item, list_location, lastSync);
                                            checkin.BranchId = item.location.IDs;
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

                                        #region Driver
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

                                        #region Branch
                                        if (item.location != null)
                                        {
                                            if (item.location.IDs != 0)
                                            {
                                                Branch(item, list_location, lastSync);
                                                checkin_update.BranchId = item.location.IDs;
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
                #endregion
            }
            else//Receipt
            {
                #region Receipt
                if (protocol.protocol_id == 30001)
                {
                    var xdata = ((JArray)protocol.protocol_data.lsreceipt).ToObject<List<Bill>>();

                    try
                    {
                        List<int> list_bill_add = new List<int>();
                        List<int> list_bill_update = new List<int>();
                        List<int> list_bill_remove = new List<int>();
                        List<int> list_customer = new List<int>();
                        List<int> list_location = new List<int>();
                        List<int> list_sales = new List<int>();
                        List<int> list_product = new List<int>();
                        List<int> list_coffer = new List<int>();
                        List<int> list_country = new List<int>();
                        List<int> list_city = new List<int>();
                        List<int> list_voucher = new List<int>();

                        //List chua nhung item da xu ly, de tranh xu ly lai vao cac lan sau
                        List<int> list_customer_update = new List<int>();
                        List<int> list_location_updated = new List<int>();
                        List<int> list_sales_updated = new List<int>();
                        List<int> list_product_updated = new List<int>();
                        List<int> list_coffer_updated = new List<int>();
                        List<int> list_country_updated = new List<int>();
                        List<int> list_city_updated = new List<int>();
                        List<int> list_voucher_updated = new List<int>();

                        #region GetDataAPI
                        //Get Id data from API
                        foreach (var item in xdata)
                        {
                            if (item.Status)//Neu status = 1
                            {
                                //Neu ko co trong DB cho vao danh sach Insert
                                if (!list_bill_add.Contains(item.IDs))
                                    if (_context.Receipts.Where(x => x.IDs == item.IDs).Count() == 0)
                                        list_bill_add.Add(item.IDs);

                                //Neu da ton tai trong DB thi cho vao danh sach Update
                                if (!list_bill_update.Contains(item.IDs))
                                    if (_context.Receipts.Where(x => x.IDs == item.IDs).Count() > 0)
                                        list_bill_update.Add(item.IDs);
                            }
                            else //Status = 0, Update Checkin_Id co Status = 0
                            {
                                if (!list_bill_remove.Contains(item.IDs))
                                    if (_context.Receipts.Where(x => x.IDs == item.IDs).Count() > 0)
                                        list_bill_remove.Add(item.IDs);
                            }

                            if (item.customer != null)
                            {
                                if (!list_customer.Contains(item.customer.IDs))
                                    if (_context.tblCustomers.Where(x => x.IDs == item.customer.IDs).Count() == 0)
                                        list_customer.Add(item.customer.IDs);

                                if (item.customer.country != null)
                                {
                                    if (!list_country.Contains(item.customer.country.IDs))
                                        if (_context.tblCountrys.Where(x => x.IDs == item.customer.country.IDs).Count() == 0)
                                            list_country.Add(item.customer.country.IDs);

                                    if (item.customer.country.cityList != null)
                                    {
                                        foreach (var c in item.customer.country.cityList)
                                        {
                                            if (!list_city.Contains(c.IDs))
                                                if (_context.tblCitys.Where(x => x.IDs == c.IDs).Count() == 0)
                                                    list_city.Add(c.IDs);
                                        }
                                    }
                                }
                            }

                            if (item.branch != null)
                            {
                                if (!list_location.Contains(item.branch.IDs))
                                    if (_context.Branchs.Where(x => x.IDs == item.branch.IDs).Count() == 0)
                                        list_location.Add(item.branch.IDs);
                            }

                            if (item.lst_sale_staff != null)
                            {
                                foreach (var s in item.lst_sale_staff)
                                {
                                    if (!list_sales.Contains(s.SalemanId))
                                        if (_context.Salemans.Where(x => x.IDs == s.SalemanId).Count() == 0)
                                            list_sales.Add(s.SalemanId);
                                }
                            }

                            if (item.lstprd != null)
                            {
                                foreach (var s in item.lstprd)
                                {
                                    if (!list_product.Contains(s.Id))
                                        if (_context.Products.Where(x => x.IDs == s.Id).Count() == 0)
                                            list_product.Add(s.Id);

                                    if (s.coffer != null)
                                    {
                                        if (!list_coffer.Contains(s.coffer.IDs))
                                            if (_context.Coffers.Where(x => x.IDs == s.coffer.IDs).Count() == 0)
                                                list_coffer.Add(s.coffer.IDs);
                                    }
                                }
                            }


                            if (item.lstDiscount != null)
                            {
                                foreach (var d in item.lstDiscount)
                                {
                                    if (!list_voucher.Contains(d.VoucherId))
                                        if (_context.Vouchers.Where(x => x.IDs == d.VoucherId).Count() == 0)
                                            list_voucher.Add(d.VoucherId);
                                }
                            }
                        }
                        #endregion

                        if (xdata != null)
                        {
                            foreach (var item in xdata)
                            {
                                if (list_bill_add.Contains(item.IDs))//Neu nam trong danh sach Insert
                                {

                                    if (!_context.Receipts.Local.Any(x => x.IDs == item.IDs))
                                    {
                                        var bill = new Receipt();
                                        bill.IDs = item.IDs;
                                        bill.Code = item.Code;
                                        bill.CreatedDate = item.CreatedDate;
                                        bill.TotalPay = item.TotalPay;
                                        bill.BillType = item.BillType;
                                        bill.Status = item.Status;
                                        bill.LastSync = lastSync;
                                        bill.CreatedAt = DateTimeOffset.Now;

                                        if (item.checkin != null)
                                        {
                                            if (item.checkin.IDs != 0)
                                                bill.CheckinId = item.checkin.IDs;
                                        }
                                        #region Customer
                                        if (item.customer != null)
                                        {
                                            if (item.customer.IDs != 0)
                                            {
                                                Customer(item, list_customer, list_customer_update, lastSync);
                                                bill.CustomerId = item.customer.IDs;

                                                if (item.customer.country != null)
                                                {
                                                    if (item.customer.country.IDs != 0)
                                                    {
                                                        Country(item.customer.country, list_country, list_country_updated, lastSync);
                                                        if (item.customer.country.cityList != null)
                                                        {
                                                            City(item.customer.country.IDs, item.customer.country.cityList, list_city, list_city_updated, lastSync);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        #endregion

                                        #region Salemans
                                        if (item.lst_sale_staff != null)
                                        {
                                            var del_bill_sale = _context.Bill_Sales
                                                .Where(x => x.BillId == item.IDs);
                                            _context.Bill_Sales.RemoveRange(del_bill_sale);

                                            foreach (var s in item.lst_sale_staff)
                                            {
                                                if (s.SalemanId != 0)
                                                {
                                                    Saleman(s, list_sales, list_sales_updated, lastSync);

                                                    var bill_sale = new Bill_Sale();
                                                    bill_sale.BillId = item.IDs;
                                                    bill_sale.SalemanId = s.SalemanId;
                                                    bill_sale.Percent = s.Percent;
                                                    bill_sale.Money = s.Money;
                                                    bill_sale.Status = s.Status;
                                                    bill_sale.LastSync = lastSync;
                                                    bill_sale.CreatedAt = DateTimeOffset.Now;
                                                    _context.Bill_Sales.Add(bill_sale);

                                                }
                                            }

                                        }
                                        #endregion

                                        #region Product
                                        if (item.lstprd != null)
                                        {
                                            var del_bill_product = _context.ReceiptDetails
                                                .Where(x => x.ReceiptId == item.IDs);
                                            _context.ReceiptDetails.RemoveRange(del_bill_product);

                                            foreach (var s in item.lstprd)
                                            {
                                                if (s.Id != 0)
                                                {
                                                    Product(s, list_product, list_product_updated, lastSync);

                                                    var receipt_detail = new ReceiptDetail();
                                                    receipt_detail.ReceiptId = item.IDs;
                                                    receipt_detail.ProductId = s.Id;
                                                    receipt_detail.Price = s.Price;
                                                    receipt_detail.Quantity = s.Quantity;
                                                    receipt_detail.Unit = s.Unit;
                                                    receipt_detail.Amount = s.Amount;
                                                    receipt_detail.PercentDiscount = s.PercentDiscount;
                                                    receipt_detail.MoneyDisCount = s.MoneyDisCount;
                                                    receipt_detail.Total = s.Total;
                                                    receipt_detail.TotalPay = s.TotalPay;
                                                    receipt_detail.Type = s.Type;
                                                    receipt_detail.SaleStatus = s.SaleStatus;
                                                    receipt_detail.IBMSCode = s.IBMSCode;
                                                    receipt_detail.LastSync = lastSync;
                                                    receipt_detail.CreatedAt = DateTimeOffset.Now;
                                                    _context.ReceiptDetails.Add(receipt_detail);

                                                    if (s.coffer != null)
                                                    {
                                                        if (s.coffer.IDs != 0)
                                                            Coffer(s.coffer, list_coffer, list_coffer_updated, lastSync);
                                                    }
                                                }
                                            }

                                        }
                                        #endregion

                                        #region Voucher
                                        if (item.lstDiscount != null)
                                        {
                                            var del_bill_discount = _context.ReceiptDiscounts
                                                .Where(x => x.ReceiptId == item.IDs);
                                            _context.ReceiptDiscounts.RemoveRange(del_bill_discount);

                                            foreach (var s in item.lstDiscount)
                                            {
                                                if (s.VoucherId != 0)
                                                {
                                                    Voucher(s, list_voucher, list_voucher_updated, lastSync);

                                                    var receipt_discount = new ReceiptDiscount();
                                                    receipt_discount.ReceiptId = item.IDs;
                                                    receipt_discount.Percent = s.Percent;
                                                    receipt_discount.DiscountType = s.DiscountType;
                                                    receipt_discount.MoneyDiscount = s.MoneyDiscount;
                                                    receipt_discount.Status = s.Status;
                                                    receipt_discount.LastSync = lastSync;
                                                    receipt_discount.CreatedAt = DateTimeOffset.Now;
                                                    _context.ReceiptDiscounts.Add(receipt_discount);

                                                }
                                            }

                                        }
                                        #endregion

                                        #region location
                                        if (item.branch != null)
                                        {
                                            if (item.branch.IDs != 0)
                                            {
                                                Bill_Branch(item, list_location, list_location_updated, lastSync);
                                                bill.BranchId = item.branch.IDs;
                                            }
                                        }
                                        #endregion

                                        _context.Receipts.Add(bill);
                                    }
                                }
                                else
                                {
                                    if (list_bill_remove.Contains(item.IDs))//Neu nam trong danh sach Remove, Update Status ve 0
                                    {
                                        var bill_remove = _context.Receipts.Where(x => x.IDs == item.IDs).FirstOrDefault();
                                        bill_remove.Status = item.Status;
                                        bill_remove.LastSync = lastSync;
                                        bill_remove.ModifiedAt = DateTimeOffset.Now;
                                    }
                                    else
                                    {
                                        if (list_bill_update.Contains(item.IDs))//Neu nam trong danh sach Update
                                        {
                                            var receipt_update = _context.Receipts.Where(x => x.IDs == item.IDs).FirstOrDefault();
                                            receipt_update.Code = item.Code;
                                            receipt_update.CreatedDate = item.CreatedDate;
                                            receipt_update.TotalPay = item.TotalPay;
                                            receipt_update.BillType = item.BillType;
                                            receipt_update.Status = item.Status;
                                            receipt_update.LastSync = lastSync;
                                            receipt_update.ModifiedAt = DateTimeOffset.Now;

                                            if (item.checkin != null)
                                            {
                                                if (item.checkin.IDs != 0)
                                                    receipt_update.CheckinId = item.checkin.IDs;
                                            }
                                            #region Customer

                                            if (item.customer != null)
                                            {
                                                if (item.customer.IDs != 0)
                                                {
                                                    Customer(item, list_customer, list_customer_update, lastSync);
                                                    receipt_update.CustomerId = item.customer.IDs;

                                                    if (item.customer.country != null)
                                                    {
                                                        if (item.customer.country.IDs != 0)
                                                        {
                                                            Country(item.customer.country, list_country, list_country_updated, lastSync);
                                                            if (item.customer.country.cityList != null)
                                                            {
                                                                City(item.customer.country.IDs, item.customer.country.cityList, list_city, list_city_updated, lastSync);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region Salemans
                                            if (item.lst_sale_staff != null)
                                            {
                                                var del_bill_sale = _context.Bill_Sales
                                                    .Where(x => x.BillId == item.IDs);
                                                _context.Bill_Sales.RemoveRange(del_bill_sale);

                                                foreach (var s in item.lst_sale_staff)
                                                {
                                                    if (s.SalemanId != 0)
                                                    {
                                                        Saleman(s, list_sales, list_sales_updated, lastSync);

                                                        var bill_sale = new Bill_Sale();
                                                        bill_sale.BillId = item.IDs;
                                                        bill_sale.SalemanId = s.SalemanId;
                                                        bill_sale.Percent = s.Percent;
                                                        bill_sale.Money = s.Money;
                                                        bill_sale.Status = s.Status;
                                                        bill_sale.LastSync = lastSync;
                                                        bill_sale.CreatedAt = DateTimeOffset.Now;
                                                        _context.Bill_Sales.Add(bill_sale);

                                                    }
                                                }

                                            }
                                            #endregion

                                            #region Product
                                            if (item.lstprd != null)
                                            {
                                                var del_bill_product = _context.ReceiptDetails
                                                    .Where(x => x.ReceiptId == item.IDs);
                                                _context.ReceiptDetails.RemoveRange(del_bill_product);

                                                foreach (var s in item.lstprd)
                                                {
                                                    if (s.Id != 0)
                                                    {
                                                        Product(s, list_product, list_product_updated, lastSync);

                                                        var receipt_detail = new ReceiptDetail();
                                                        receipt_detail.ReceiptId = item.IDs;
                                                        receipt_detail.ProductId = s.Id;
                                                        receipt_detail.Price = s.Price;
                                                        receipt_detail.Quantity = s.Quantity;
                                                        receipt_detail.Unit = s.Unit;
                                                        receipt_detail.Amount = s.Amount;
                                                        receipt_detail.PercentDiscount = s.PercentDiscount;
                                                        receipt_detail.MoneyDisCount = s.MoneyDisCount;
                                                        receipt_detail.Total = s.Total;
                                                        receipt_detail.TotalPay = s.TotalPay;
                                                        receipt_detail.Type = s.Type;
                                                        receipt_detail.SaleStatus = s.SaleStatus;
                                                        receipt_detail.IBMSCode = s.IBMSCode;
                                                        receipt_detail.LastSync = lastSync;
                                                        receipt_detail.CreatedAt = DateTimeOffset.Now;
                                                        _context.ReceiptDetails.Add(receipt_detail);

                                                        if (s.coffer != null)
                                                        {
                                                            if (s.coffer.IDs != 0)
                                                                Coffer(s.coffer, list_coffer, list_coffer_updated, lastSync);
                                                        }
                                                    }
                                                }

                                            }
                                            #endregion

                                            #region Voucher
                                            if (item.lstDiscount != null)
                                            {
                                                var del_bill_discount = _context.ReceiptDiscounts
                                                    .Where(x => x.ReceiptId == item.IDs);
                                                _context.ReceiptDiscounts.RemoveRange(del_bill_discount);

                                                foreach (var s in item.lstDiscount)
                                                {
                                                    if (s.VoucherId != 0)
                                                    {
                                                        Voucher(s, list_voucher, list_voucher_updated, lastSync);

                                                        var receipt_discount = new ReceiptDiscount();
                                                        receipt_discount.ReceiptId = item.IDs;
                                                        receipt_discount.Percent = s.Percent;
                                                        receipt_discount.DiscountType = s.DiscountType;
                                                        receipt_discount.MoneyDiscount = s.MoneyDiscount;
                                                        receipt_discount.Status = s.Status;
                                                        receipt_discount.LastSync = lastSync;
                                                        receipt_discount.CreatedAt = DateTimeOffset.Now;
                                                        _context.ReceiptDiscounts.Add(receipt_discount);

                                                    }
                                                }

                                            }
                                            #endregion

                                            #region Branch
                                            if (item.branch != null)
                                            {
                                                if (item.branch.IDs != 0)
                                                {
                                                    Bill_Branch(item, list_location, list_location_updated, lastSync);
                                                    receipt_update.BranchId = item.branch.IDs;
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
                        //end protocol.receipt 
                    }

                    catch (Exception axx)
                    {
                        mess_err += axx.Message;
                    }
                }
                #endregion
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
                var result = protocol;
                result.err_code = -1;
                result.err_message = mess_err;
                result.protocol_status = false;
                result.protocol_id = protocol.protocol_id;
                result.partner_pass = protocol.partner_pass;
                result.partner_name = protocol.partner_name;
                return Ok(result);
            }
            else
            {
                var sync = new DataSync();
                sync.LastSync = lastSync;
                sync.Status = true;
                _context.DataSyncs.Add(sync);
                _context.SaveChanges();

                var result = new Protocol
                {
                    protocol_id = protocol.protocol_id,
                    partner_pass = protocol.partner_pass,
                    partner_name = protocol.partner_name,
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
        private void Customer(Bill item, List<int> list_customer, List<int> list_customer_update, string lastSync)
        {
            if (list_customer.Contains(item.customer.IDs))//Create
            {
                var customer = new tblCustomer();
                customer.IDs = item.customer.IDs;
                customer.Name = item.customer.Name;
                customer.Email = item.customer.Email;
                customer.Phone = item.customer.Phone;
                //if (item.customer.Birthday != null)
                //    customer.Birthday = item.customer.Birthday.Value;
                customer.CountryId = item.customer.country.IDs;
                customer.LastSync = lastSync;
                customer.CreatedAt = DateTimeOffset.Now;

                _context.tblCustomers.Add(customer);
                list_customer.Remove(item.customer.IDs);
            }
            else//Update
            {
                var update = _context.tblCustomers.Where(x => x.IDs == item.customer.IDs).FirstOrDefault();
                if (update != null && !list_customer_update.Contains(item.customer.IDs))
                {
                    if (update.Code != item.customer.Code)
                    {
                        update.Code = item.customer.Code;
                        update.Name = item.customer.Name;
                        update.Email = item.customer.Email;
                        update.Phone = item.customer.Phone;
                        if(item.customer.Birthday!=null)
                            update.Birthday = item.customer.Birthday.Value;
                        update.CountryId = item.customer.country.IDs;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                        list_customer_update.Add(item.customer.IDs);
                    }
                }
            }
        }
        private void Saleman(Bill_Sale item, List<int> list_sales, List<int> list_sales_update, string lastSync)
        {
            if (list_sales.Contains(item.SalemanId))//Create
            {
                var sales = new Saleman();
                sales.IDs = item.SalemanId;
                sales.Name = item.SalemanName;
                sales.LastSync = lastSync;
                sales.CreatedAt = DateTimeOffset.Now;

                _context.Salemans.Add(sales);
                list_sales.Remove(item.SalemanId);
            }
            else//Update
            {
                var update = _context.Salemans.Where(x => x.IDs == item.SalemanId).FirstOrDefault();
                if (update != null && !list_sales_update.Contains(item.SalemanId))
                {
                    if (update.Name != item.SalemanName)
                    {
                        update.Name = item.SalemanName;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                        list_sales_update.Add(item.SalemanId);
                    }
                }
            }
        }
        private void Bill_Branch(Bill item, List<int> list_location,List<int> list_location_update, string lastSync)
        {
            if (list_location.Contains(item.branch.IDs))//Create
            {
                var location = new Branch();
                location.IDs = item.branch.IDs;
                location.BranchName = item.branch.BranchName;
                location.Address = item.branch.Address;
                location.LastSync = lastSync;
                location.CreatedAt = DateTimeOffset.Now;

                _context.Branchs.Add(location);
                list_location.Remove(item.branch.IDs);
            }
            else//Update
            {
                var update = _context.Branchs.Where(x => x.IDs == item.branch.IDs).FirstOrDefault();
                if (update != null && !list_location_update.Contains(item.branch.IDs))
                {
                    if (update.BranchName != item.branch.BranchName)
                    {
                        update.BranchName = item.branch.BranchName;
                        update.Address = item.branch.Address;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                        list_location_update.Add(item.branch.IDs);
                    }
                }
            }
        }
        private void Product(Bill_Product item, List<int> list_product, List<int> list_product_update, string lastSync)
        {
            if (list_product.Contains(item.Id))//Create
            {
                var product = new Product();
                product.IDs = item.Id;
                product.Code = item.ProductCode;
                product.Name = item.ProductName;
                product.LastSync = lastSync;
                product.CreatedAt = DateTimeOffset.Now;

                _context.Products.Add(product);
                list_product.Remove(item.Id);
            }
            else//Update
            {
                var update = _context.Products.Where(x => x.IDs == item.Id).FirstOrDefault();
                if (update != null && !list_product_update.Contains(item.Id))
                {
                    if (update.Name != item.ProductName)
                    {
                        update.Name = item.ProductName;
                        update.Code = item.ProductCode;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                        list_product_update.Add(item.Id);
                    }
                }
            }
        }
        private void Coffer(Coffer item, List<int> list_coffer, List<int> list_coffer_update, string lastSync)
        {
            if (list_coffer.Contains(item.IDs))//Create
            {
                var coffer = new Coffer();
                coffer.IDs = item.IDs;
                coffer.Name = item.Name;
                coffer.BranchId = item.BranchId;
                coffer.Type = item.Type;
                coffer.LastSync = lastSync;
                coffer.CreatedAt = DateTimeOffset.Now;

                _context.Coffers.Add(coffer);
                list_coffer.Remove(item.IDs);
            }
            else//Update
            {
                var update = _context.Coffers.Where(x => x.IDs == item.IDs).FirstOrDefault();
                if (update != null && !list_coffer_update.Contains(item.IDs))
                {
                    if (update.Name != item.Name)
                    {
                        update.Name = item.Name;
                        update.Type = item.Type;
                        update.BranchId = item.BranchId;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                        list_coffer_update.Add(item.IDs);
                    }
                }
            }
        }
        private void Country(Country item, List<int> list_country, List<int> list_country_updated, string lastSync)
        {
            if (list_country.Contains(item.IDs))//Create
            {
                var country = new tblCountry();
                country.IDs = item.IDs;
                country.Code = item.Code;
                country.Name = item.Name;
                country.LastSync = lastSync;
                country.CreatedAt = DateTimeOffset.Now;

                _context.tblCountrys.Add(country);
                list_country.Remove(item.IDs);
            }
            else//Update
            {
                var update = _context.tblCountrys.Where(x => x.IDs == item.IDs).FirstOrDefault();
                if (update != null && !list_country_updated.Contains(item.IDs))
                {
                    if (update.Code != item.Code)
                    {
                        update.Code = item.Code;
                        update.Name = item.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                        list_country_updated.Add(item.IDs);
                    }
                }
            }
        }
        private void City(int CountryId, List<City> item, List<int> list_city, List<int> list_city_updated, string lastSync)
        {
            foreach (var c in item)
            {
                if (list_city.Contains(c.IDs))//Create
                {
                    var city = new tblCity();
                    city.IDs = c.IDs;
                    city.CountryId = CountryId;
                    city.Name = c.Name;
                    city.LastSync = lastSync;
                    city.CreatedAt = DateTimeOffset.Now;

                    _context.tblCitys.Add(city);
                    list_city.Remove(c.IDs);
                }
                else//Update
                {
                    var update = _context.tblCitys.Where(x => x.IDs == c.IDs).FirstOrDefault();
                    if (update != null && !list_city_updated.Contains(c.IDs))
                    {
                        if (update.Name != c.Name)
                        {
                            update.Name = c.Name;
                            update.CountryId = CountryId;
                            update.LastSync = lastSync;
                            update.ModifiedAt = DateTimeOffset.Now;
                            list_city_updated.Add(c.IDs);
                        }
                    }
                }
            }
        }
        private void Voucher(Discount c, List<int> list_voucher, List<int> list_voucher_updated, string lastSync)
        {
            if (list_voucher.Contains(c.VoucherId))//Create
            {
                var voucher = new Voucher();
                voucher.IDs = c.VoucherId;
                voucher.Code = c.VoucherName;
                voucher.LastSync = lastSync;
                voucher.CreatedAt = DateTimeOffset.Now;

                _context.Vouchers.Add(voucher);
                list_voucher.Remove(c.VoucherId);
            }
            else//Update
            {
                var update = _context.Vouchers.Where(x => x.IDs == c.VoucherId).FirstOrDefault();
                if (update != null && !list_voucher_updated.Contains(c.VoucherId))
                {
                    if (update.Code != c.VoucherName)
                    {
                        update.Code = c.VoucherName;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTimeOffset.Now;
                        list_voucher_updated.Add(c.VoucherId);
                    }
                }
            }
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
        private void Branch(CheckIn item, List<int> list_location, string lastSync)
        {
            if (list_location.Contains(item.location.IDs))
            {
                var branch = new Branch();
                branch.IDs = item.location.IDs;
                branch.BranchName = item.location.BranchName;
                branch.Address = item.location.Address;
                branch.LastSync = lastSync;
                branch.CreatedAt = DateTimeOffset.Now;

                _context.Branchs.Add(branch);
                list_location.Remove(item.location.IDs);
            }
            else//Update
            {
                var update = _context.Branchs.Where(x => x.IDs == item.location.IDs).FirstOrDefault();
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
