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
    [RoutePrefix("api/pentamic")]
    public class PentamicController : ApiController
    {
        private const int protocol_checkin = 30000;
        private const int protocol_receipt = 30001;
        private const int protocol_stock = 30002;
        private const int protocol_product = 20000;
        private const int protocol_inventory = 30003;
        private const int protocol_coffer = 30004;
        private const int protocol_store = 30005;

        enum ProtocolName
        {
            CheckIn = 30000,
            Receipt = 30001,
            Stock = 30002,
            Product = 20000,
            Inventory = 30003,
            Coffer = 30004,
            Store = 30005
        };
        private ApplicationDbContext _context;
        public PentamicController()
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
            string mess_err = "", mess_id = "", lastSync = DateTime.Now.ToString("ddMMyyyyHHmmss");
            int totalRecord = 0, recordSuccess = 0;//Dung de dem tong so ban ghi nhan duoc, va so ban ghi thanh cong

            try
            {
                Log.Information("Request: {MediaType}", Request.Content.Headers.ContentType.MediaType);
                Log.Information("Received package: {Package}", JsonConvert.SerializeObject(protocol));

                var enableAPI = _context.SettingAPIs.FirstOrDefault();
                if (!enableAPI.Enable)
                {
                    var result = protocol;
                    result.protocol_status = false;
                    result.err_code = -1;
                    result.err_message = "API PENTAMIC is disable";

                    AddLogDataSync(_context, false, lastSync, totalRecord, recordSuccess, mess_id, "API PENTAMIC is disable", protocol);

                    return Ok(result);
                }
                else
                {
                    List<tmpReceipt> returnReceipt = new List<tmpReceipt>();//Luu tru nhung ban ghi bi loi
                    List<tmpCheckIn> returnCheckin = new List<tmpCheckIn>();//Luu tru nhung ban ghi bi loi
                    List<tmpProduct> returnProduct = new List<tmpProduct>();//Luu tru nhung ban ghi bi loi
                    List<tmpStock> returnStock = new List<tmpStock>();//Luu tru nhung ban ghi bi loi
                    List<tmpInventory> returnInventory = new List<tmpInventory>();//Luu tru nhung ban ghi bi loi
                    List<tmpCoffer> returnCoffer = new List<tmpCoffer>();//Luu tru nhung ban ghi bi loi
                    List<tmpBranch> returnStore = new List<tmpBranch>();//Luu tru nhung ban ghi bi loi

                    if (protocol.protocol_id == protocol_checkin)//CheckIn
                    {
                        SyncCheckIn(protocol, out mess_err, out mess_id, out totalRecord, out recordSuccess, out returnCheckin, lastSync);
                    }
                    else if (protocol.protocol_id == protocol_receipt)//Receipt
                    {
                        SyncReceipt(protocol, out mess_err, out mess_id, out totalRecord, out recordSuccess, out returnReceipt, lastSync);
                    }
                    else if (protocol.protocol_id == protocol_product)//Product
                    {
                        SyncProduct(protocol, out mess_err, out mess_id, out totalRecord, out recordSuccess, out returnProduct, lastSync);
                    }
                    else if (protocol.protocol_id == protocol_stock)//Stock
                    {
                        SyncStock(protocol, out mess_err, out mess_id, out totalRecord, out recordSuccess, out returnStock, lastSync);
                    }
                    else if (protocol.protocol_id == protocol_inventory)//Inventory
                    {
                        SyncInventory(protocol, out mess_err, out mess_id, out totalRecord, out recordSuccess, out returnInventory, lastSync);
                    }
                    else if (protocol.protocol_id == protocol_coffer)//Coffer
                    {
                        SyncCoffer(protocol, out mess_err, out mess_id, out totalRecord, out recordSuccess, out returnCoffer, lastSync);
                    }
                    else if (protocol.protocol_id == protocol_store)//Store
                    {
                        SyncStore(protocol, out mess_err, out mess_id, out totalRecord, out recordSuccess, out returnStore, lastSync);
                    }

                    if (mess_err != "")
                    {
                        using (var context = new ApplicationDbContext())
                        {
                            AddLogDataSync(context, false, lastSync, totalRecord, recordSuccess, mess_id, mess_err, protocol);
                        }
                        var result = protocol;
                        result.err_code = -1;
                        result.err_message = mess_err;
                        result.protocol_status = false;
                        result.protocol_id = protocol.protocol_id;
                        result.partner_pass = protocol.partner_pass;
                        result.partner_name = protocol.partner_name;

                        if (protocol.protocol_id == protocol_receipt)
                        {
                            result.protocol_data = new { lsreceipt = returnReceipt };
                        }
                        else if (protocol.protocol_id == protocol_checkin)
                        {
                            result.protocol_data = new { checkin_list = returnCheckin };
                        }
                        else if (protocol.protocol_id == protocol_product)
                        {
                            result.protocol_data = new { lstprd = returnProduct };
                        }
                        else if (protocol.protocol_id == protocol_stock)
                        {
                            result.protocol_data = new { lstprd = returnStock };
                        }
                        else if (protocol.protocol_id == protocol_inventory)
                        {
                            result.protocol_data = new { product_history = returnInventory };
                        }
                        else if (protocol.protocol_id == protocol_coffer)
                        {
                            result.protocol_data = new { coffer_list = returnCoffer };
                        }
                        else if (protocol.protocol_id == protocol_store)
                        {
                            result.protocol_data = new { lstBranch = returnStore };
                        }
                        Log.Information("Return package: {Package}", JsonConvert.SerializeObject(result));
                        return Ok(result);
                    }
                    else
                    {
                        AddLogDataSync(_context, true, lastSync, totalRecord, recordSuccess, mess_id, mess_err, protocol);

                        var result = new Protocol();
                        result.protocol_id = protocol.protocol_id;
                        result.partner_pass = protocol.partner_pass;
                        result.partner_name = protocol.partner_name;
                        result.protocol_status = true;
                        result.err_code = 1;
                        result.err_message = "SUCCESS";

                        Log.Information("Return package: {Package}", JsonConvert.SerializeObject(result));
                        return Ok(result);
                    }
                }
            }
            catch (Exception ax)
            {
                using (var context = new ApplicationDbContext())
                {
                    AddLogDataSync(context, false, lastSync, totalRecord, recordSuccess, mess_id, GetExceptionDetail(ax), protocol);
                }

                var result = new Protocol();
                result.protocol_id = protocol.protocol_id;
                result.partner_pass = protocol.partner_pass;
                result.partner_name = protocol.partner_name;
                result.protocol_status = false;
                result.err_code = -1;
                result.err_message = GetExceptionDetail(ax);

                Log.Information("Return package: {Package}", JsonConvert.SerializeObject(result));
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
        private void Supplier(tmpSupplier item, List<int> list_supplier, List<int> list_supplier_update, string lastSync)
        {
            if (list_supplier.Contains(item.IDs))//Create
            {
                var supplier = new Supplier();
                supplier.IDs = item.IDs;
                supplier.Name = item.Name;
                supplier.Code = item.Code;
                supplier.LastSync = lastSync;
                supplier.CreatedAt = DateTime.Now;

                _context.Suppliers.Add(supplier);
                list_supplier.Remove(item.IDs);
            }
            else//Update
            {
                var update = _context.Suppliers.Where(x => x.IDs == item.IDs).FirstOrDefault();
                if (update != null && !list_supplier_update.Contains(item.IDs))
                {
                    if (update.Code != item.Code || update.Name != item.Name)
                    {
                        update.Code = item.Code;
                        update.Name = item.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_supplier_update.Add(item.IDs);
                    }
                }
            }
        }
        private void Customer(tmpReceipt item, List<int> list_customer, List<int> list_customer_update, string lastSync)
        {
            if (list_customer.Contains(item.customer.IDs))//Create
            {
                var customer = new Customer();
                customer.IDs = item.customer.IDs;
                customer.Name = item.customer.Name;
                customer.Email = item.customer.Email;
                customer.Phone = item.customer.Phone;
                //if (item.customer.Birthday != null)
                //    customer.Birthday = item.customer.Birthday.Value;
                customer.CountryId = item.customer.country.IDs;
                customer.LastSync = lastSync;
                customer.CreatedAt = DateTime.Now;

                _context.tblCustomers.Add(customer);
                list_customer.Remove(item.customer.IDs);
            }
            else//Update
            {
                var update = _context.tblCustomers.Where(x => x.IDs == item.customer.IDs).FirstOrDefault();
                if (update != null && !list_customer_update.Contains(item.customer.IDs))
                {
                    if (update.Code != item.customer.Code || update.Name != item.customer.Name)
                    {
                        update.Code = item.customer.Code;
                        update.Name = item.customer.Name;
                        update.Email = item.customer.Email;
                        update.Phone = item.customer.Phone;
                        //if (item.customer.Birthday != null)
                        //    update.Birthday = item.customer.Birthday.Value;
                        update.CountryId = item.customer.country.IDs;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_customer_update.Add(item.customer.IDs);
                    }
                }
            }
        }

        private void FeePortName(tmpFeePort item, List<int> list_fee, List<int> list_fee_update, string lastSync)
        {
            if (list_fee.Contains(item.IDs))//Create
            {
                var fee = new ContactFeePort();
                fee.IDs = item.IDs;
                fee.Name = item.Name;
                fee.Email = item.Email;
                fee.Phone = item.Phone;
                fee.LastSync = lastSync;
                fee.CreatedAt = DateTime.Now;
                _context.ContactFeePorts.Add(fee);
                list_fee.Remove(item.IDs);
            }
            else
            {
                var update = _context.ContactFeePorts.Where(x => x.IDs == item.IDs).FirstOrDefault();
                if (update != null && !list_fee_update.Contains(item.IDs))
                {
                    if (update.Name != item.Name)
                    {
                        update.Name = item.Name;
                        update.Email = item.Email;
                        update.Phone = item.Phone;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_fee_update.Add(item.IDs);
                    }
                }
            }
        }
        private void Saleman(ReceiptSalesStaff item, List<int> list_sales, List<int> list_sales_update, string lastSync)
        {
            if (list_sales.Contains(item.SalemanId))//Create
            {
                var sales = new Saleman();
                sales.IDs = item.SalemanId;
                sales.Name = item.SalemanName;
                sales.LastSync = lastSync;
                sales.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                        list_sales_update.Add(item.SalemanId);
                    }
                }
            }
        }
        private void Bill_Branch(tmpReceipt item, List<int> list_location, List<int> list_location_update, string lastSync)
        {
            if (list_location.Contains(item.branch.IDs))//Create
            {
                var location = new Branch();
                location.IDs = item.branch.IDs;
                location.BranchCode = item.branch.BranchCode;
                location.BranchName = item.branch.BranchName;
                location.Address = item.branch.Address;
                location.LastSync = lastSync;
                location.CreatedAt = DateTime.Now;

                _context.Branchs.Add(location);
                list_location.Remove(item.branch.IDs);
            }
            else//Update
            {
                var update = _context.Branchs.Where(x => x.IDs == item.branch.IDs).FirstOrDefault();
                if (update != null && !list_location_update.Contains(item.branch.IDs))
                {
                    if (update.BranchCode != item.branch.BranchCode)
                    {
                        update.BranchCode = item.branch.BranchCode;
                        update.BranchName = item.branch.BranchName;
                        update.Address = item.branch.Address;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_location_update.Add(item.branch.IDs);
                    }
                }
            }
        }
        private void Product(tmpReceiptDetail item, List<int> list_product, List<int> list_product_update, string lastSync)
        {
            if (list_product.Contains(item.Id))//Create
            {
                var product = new Product();
                product.IDs = item.Id;
                product.Code = item.ProductCode;
                product.Name = item.ProductName;
                product.LastSync = lastSync;
                product.CreatedAt = DateTime.Now;

                _context.Products.Add(product);
                list_product.Remove(item.Id);
            }
            else//Update
            {
                var update = _context.Products.Where(x => x.IDs == item.Id).FirstOrDefault();
                if (update != null && !list_product_update.Contains(item.Id))
                {
                    if (update.Name != item.ProductName || update.Code != item.ProductCode)
                    {
                        update.Name = item.ProductName;
                        update.Code = item.ProductCode;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_product_update.Add(item.Id);
                    }
                }
            }
        }
        private void Coffer(Coffer item, List<int> list_coffer, List<int> list_coffer_update, string lastSync,string branchCode)
        {
            if (list_coffer.Contains(item.IDs))//Create
            {
                var coffer = new Coffer();
                coffer.IDs = item.IDs;
                coffer.Name = item.Name;
                coffer.BranchId = item.BranchId;
                coffer.Type = item.Type;
                coffer.LastSync = lastSync;
                coffer.CreatedAt = DateTime.Now;
                if(branchCode!="")
                    coffer.BranchCode = branchCode;
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
                        if (branchCode != "")
                            update.BranchCode = branchCode;
                        //else update.BranchCode = null;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_coffer_update.Add(item.IDs);
                    }
                }
            }
        }
        private void Country(tmpCountry item, List<int> list_country, List<int> list_country_updated, string lastSync)
        {
            if (list_country.Contains(item.IDs))//Create
            {
                var country = new Country();
                country.IDs = item.IDs;
                country.Code = item.Code;
                country.Name = item.Name;
                country.LastSync = lastSync;
                country.CreatedAt = DateTime.Now;

                _context.tblCountrys.Add(country);
                list_country.Remove(item.IDs);
            }
            else//Update
            {
                var update = _context.tblCountrys.Where(x => x.IDs == item.IDs).FirstOrDefault();
                if (update != null && !list_country_updated.Contains(item.IDs))
                {
                    if (update.Code != item.Code || update.Name != item.Name)
                    {
                        update.Code = item.Code;
                        update.Name = item.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_country_updated.Add(item.IDs);
                    }
                }
            }
        }
        private void City(int CountryId, List<tmpCity> item, List<int> list_city, List<int> list_city_updated, string lastSync)
        {
            foreach (var c in item)
            {
                if (list_city.Contains(c.IDs))//Create
                {
                    var city = new City();
                    city.IDs = c.IDs;
                    city.CountryId = CountryId;
                    city.Name = c.Name;
                    city.LastSync = lastSync;
                    city.CreatedAt = DateTime.Now;

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
                            update.ModifiedAt = DateTime.Now;
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
                voucher.Percent = c.Percent;
                voucher.DiscountType = c.DiscountType;
                voucher.MoneyDiscount = c.MoneyDiscount;
                voucher.LastSync = lastSync;
                voucher.CreatedAt = DateTime.Now;

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
                        update.Percent = c.Percent;
                        update.DiscountType = c.DiscountType;
                        update.MoneyDiscount = c.MoneyDiscount;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_voucher_updated.Add(c.VoucherId);
                    }
                }
            }
        }
        private void BankAccount(ReceiptPayment item, List<int> list_bank_account, List<int> list_bank_account_updated, string lastSync)
        {
            if (list_bank_account.Contains(item.BankAccount.IDs))//Create
            {
                var bank = new BankAccount();
                bank.IDs = item.BankAccount.IDs;
                bank.Name = item.BankAccount.Name;
                bank.LastSync = lastSync;
                bank.CreatedAt = DateTime.Now;

                _context.BankAccounts.Add(bank);
                list_bank_account.Remove(item.BankAccount.IDs);
            }
            else//Update
            {
                var update = _context.BankAccounts.Where(x => x.IDs == item.BankAccount.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.Name != item.BankAccount.Name)
                    {
                        update.Name = item.BankAccount.Name;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_bank_account_updated.Add(item.BankAccount.IDs);
                    }
                }
            }
        }
        private void CardType(tmpCheckIn item, List<int> list_card_type, string lastSync)
        {
            if (list_card_type.Contains(item.card_type.IDs))//Create
            {
                var cardtype = new CardType();
                cardtype.IDs = item.card_type.IDs;
                cardtype.Name = item.card_type.Name;
                cardtype.LastSync = lastSync;
                cardtype.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void CardMaker(tmpCheckIn item, List<int> list_card_maker, List<int> list_card_maker_updated, string lastSync)
        {
            if (list_card_maker.Contains(item.card_maker.IDs))
            {
                var cardmaker = new CardMaker();
                cardmaker.IDs = item.card_maker.IDs;
                cardmaker.Name = item.card_maker.Name;
                cardmaker.LastSync = lastSync;
                cardmaker.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;

                        list_card_maker_updated.Add(item.card_maker.IDs);
                    }
                }
            }
        }
        private void PartnerGroup(tmpCheckIn item, List<int> list_partner_group, string lastSync)
        {
            if (list_partner_group.Contains(item.partner_group.IDs))
            {
                var partnerGroup = new PartnerGroup();
                partnerGroup.IDs = item.partner_group.IDs;
                partnerGroup.Name = item.partner_group.Name;
                partnerGroup.LastSync = lastSync;
                partnerGroup.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void Partner(tmpCheckIn item, List<int> list_partner, string lastSync)
        {
            if (list_partner.Contains(item.partner_group.partner.IDs))
            {
                var partner = new Partner();
                partner.IDs = item.partner_group.partner.IDs;
                partner.Name = item.partner_group.partner.Name;
                partner.LastSync = lastSync;
                partner.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void Driver(tmpCheckIn item, List<int> list_driver, string lastSync)
        {
            if (list_driver.Contains(item.driver.IDs))
            {
                var driver = new Driver();
                driver.IDs = item.driver.IDs;
                driver.Name = item.driver.Name;
                driver.LastSync = lastSync;
                driver.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void TourGuide(tmpCheckIn item, List<int> list_tour, string lastSync)
        {
            if (list_tour.Contains(item.tour_guide.IDs))
            {
                var tour = new TourGuide();
                tour.IDs = item.tour_guide.IDs;
                tour.Name = item.tour_guide.Name;
                tour.LastSync = lastSync;
                tour.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void Visitor(tmpCheckIn item, List<int> list_visit, string lastSync)
        {
            if (list_visit.Contains(item.visitor_type.IDs))
            {
                var visit = new VisitorType();
                visit.IDs = item.visitor_type.IDs;
                visit.Name = item.visitor_type.Name;
                visit.LastSync = lastSync;
                visit.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
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
                payee.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void Nationality(CheckInDetail i, List<int> list_nationality, string lastSync)
        {
            if (list_nationality.Contains(i.NationalityId))
            {
                var national = new Nationality();
                national.IDs = i.NationalityId;
                national.Name = i.Nationality;
                national.LastSync = lastSync;
                national.CreatedAt = DateTime.Now;

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
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void Branch(tmpCheckIn item, List<int> list_location, string lastSync)
        {
            if (list_location.Contains(item.location.IDs))
            {
                var branch = new Branch();
                branch.IDs = item.location.IDs;
                branch.BranchCode = item.location.BranchCode;
                branch.BranchName = item.location.BranchName;
                branch.Address = item.location.Address;
                branch.LastSync = lastSync;
                branch.CreatedAt = DateTime.Now;

                _context.Branchs.Add(branch);
                list_location.Remove(item.location.IDs);
            }
            else//Update
            {
                var update = _context.Branchs.Where(x => x.IDs == item.location.IDs).FirstOrDefault();
                if (update != null)
                {
                    if (update.BranchCode != item.location.BranchCode)
                    {
                        update.BranchCode = item.location.BranchCode;
                        update.BranchName = item.location.BranchName;
                        update.Address = item.location.Address;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                    }
                }
            }
        }
        private void Stock(tmpStock item, string lastSync)
        {
            var stock = new BalanceStock();
            stock.ProductId = item.ProductId;
            stock.ProductCode = item.ProductCode;
            stock.Price = item.Price;
            stock.CostPrice = item.CostPrice;
            stock.CreateDate = item.CreateDate;
            stock.Quantity = item.Quantity;
            stock.Type = item.Type;
            stock.CofferId = item.Coffer.IDs;
            stock.BranchId = item.Branch[0].IDs;
            stock.BranchCode = item.Branch[0].BranchCode;
            stock.LastSync = lastSync;
            stock.Status = true;
            stock.CreatedAt = DateTime.Now;
            _context.BalanceStocks.Add(stock);
        }
        private void ProductGroup(tmpProductGroup small_group, tmpProductGroup middle_group, tmpProductGroup big_group, List<int> list_product_group_add, List<int> list_product_group_update, List<int> list_product_group_updated, string lastSync)
        {
            //small_group
            if (list_product_group_add.Contains(small_group.IDs))//Create
            {
                var prdGroup = new ProductGroup();
                prdGroup.IDs = small_group.IDs;
                prdGroup.Name = small_group.Name;
                prdGroup.Type = 3;
                prdGroup.ParentGroupId = small_group.ParentGroupId;
                prdGroup.LastSync = lastSync;
                prdGroup.CreatedAt = DateTime.Now;

                _context.ProductGroups.Add(prdGroup);
                list_product_group_add.Remove(small_group.IDs);
            }
            else//Update
            {
                var update = _context.ProductGroups.Where(x => x.IDs == small_group.IDs).FirstOrDefault();
                if (update != null && list_product_group_update.Contains(small_group.IDs) && !list_product_group_updated.Contains(small_group.IDs))
                {
                    if (update.Name != small_group.Name)
                    {
                        update.Name = small_group.Name;
                        update.Type = 3;
                        update.ParentGroupId = small_group.ParentGroupId;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_product_group_updated.Add(small_group.IDs);
                    }
                }
            }

            //middle_group
            if (list_product_group_add.Contains(middle_group.IDs))//Create
            {
                var prdGroup = new ProductGroup();
                prdGroup.IDs = middle_group.IDs;
                prdGroup.Name = middle_group.Name;
                prdGroup.Type = 2;
                prdGroup.ParentGroupId = middle_group.ParentGroupId;
                prdGroup.LastSync = lastSync;
                prdGroup.CreatedAt = DateTime.Now;

                _context.ProductGroups.Add(prdGroup);
                list_product_group_add.Remove(middle_group.IDs);
            }
            else//Update
            {
                var update = _context.ProductGroups.Where(x => x.IDs == middle_group.IDs).FirstOrDefault();
                if (update != null && list_product_group_update.Contains(middle_group.IDs) && !list_product_group_updated.Contains(middle_group.IDs))
                {
                    if (update.Name != middle_group.Name)
                    {
                        update.Type = 2;
                        update.Name = middle_group.Name;
                        update.ParentGroupId = middle_group.ParentGroupId;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_product_group_updated.Add(middle_group.IDs);
                    }
                }
            }
            //big_group
            if (list_product_group_add.Contains(big_group.IDs))//Create
            {
                var prdGroup = new ProductGroup();
                prdGroup.IDs = big_group.IDs;
                prdGroup.Name = big_group.Name;
                prdGroup.Type = 1;
                prdGroup.ParentGroupId = big_group.ParentGroupId;
                prdGroup.LastSync = lastSync;
                prdGroup.CreatedAt = DateTime.Now;

                _context.ProductGroups.Add(prdGroup);
                list_product_group_add.Remove(big_group.IDs);
            }
            else//Update
            {
                var update = _context.ProductGroups.Where(x => x.IDs == big_group.IDs).FirstOrDefault();
                if (update != null && list_product_group_update.Contains(big_group.IDs) && !list_product_group_updated.Contains(big_group.IDs))
                {
                    if (update.Name != big_group.Name)
                    {
                        update.Type = 1;
                        update.Name = big_group.Name;
                        update.ParentGroupId = big_group.ParentGroupId;
                        update.LastSync = lastSync;
                        update.ModifiedAt = DateTime.Now;
                        list_product_group_updated.Add(big_group.IDs);
                    }
                }
            }
        }
        private void AddLogDataSync(ApplicationDbContext _context, bool status, string lastSync, int totalRecord, int recordSuccess, string mess_id, string mess_err, Protocol protocol)
        {
            var sync = new DataSync();
            sync.LastSync = lastSync;
            sync.Status = status;
            sync.Message = mess_err;
            sync.Type = protocol.protocol_id == protocol_receipt ? "Receipt" :
                protocol.protocol_id == protocol_checkin ? "CheckIn" :
                protocol.protocol_id == protocol_coffer ? "Coffer" :
                protocol.protocol_id == protocol_product ? "Product" :
                protocol.protocol_id == protocol_stock ? "Stock" :
                protocol.protocol_id == protocol_inventory ? "Inventory" :
                protocol.protocol_id == protocol_store ? "Store" : "";
            sync.RecordIdFailure = mess_id;
            sync.TotalRecord = totalRecord;
            sync.RecordSuccess = recordSuccess;
            _context.DataSyncs.Add(sync);
            _context.SaveChanges();
        }
        private void SyncStock(Protocol protocol, out string mess_err, out string mess_id, out int TotalRecord, out int RecordSuccess, out List<tmpStock> returnStock, string lastSync)
        {
            mess_err = ""; mess_id = ""; TotalRecord = 0; RecordSuccess = 0;
            returnStock = new List<tmpStock>();
            //SYnc Ton KHo
            #region Stock
            try
            {
                string input_err = "";
                var xdata = ((JArray)protocol.protocol_data.lstprd).ToObject<List<tmpStock>>();
                List<int> list_stock_add = new List<int>();
                List<int> list_stock_update = new List<int>();

                if (xdata != null)
                {
                    int id_Coffer = 0;
                    int id_Branch = 0;
                    foreach (var item in xdata)
                    {
                        TotalRecord++;//Tang so ban ghi nhan duoc len
                        input_err = "";
                        try
                        {
                            _context = new ApplicationDbContext();

                            if (item.ProductId == 0)
                                input_err = "Product Id is null";
                            if (Convert.ToString(item.Price) == "")
                                input_err = "Price Product is null";
                            if (Convert.ToString(item.CostPrice) == "")
                                input_err = "CostPrice Product is null";
                            if (Convert.ToString(item.CreateDate) == "")
                                input_err = "CreateDate Product is null";
                            if (Convert.ToString(item.Quantity) == "")
                                input_err = "Quantity Product is null";
                            if (Convert.ToString(item.Type) == "")
                                input_err = "Type Product is null";
                            if (Convert.ToString(item.Coffer.IDs) == "")
                                input_err = "Coffer Id is null";
                            if (Convert.ToString(item.Branch[0].IDs) == "")
                                input_err = "Branch Id is null";

                            if (input_err != "")
                                throw new Exception(input_err);
                            else
                            {
                                id_Coffer = item.Coffer.IDs;
                                id_Branch = item.Branch[0].IDs;

                                var check_Stock = _context.BalanceStocks.Where(x => x.ProductCode == item.ProductCode
                                  && x.Type == item.Type && x.CofferId == id_Coffer && x.BranchId == id_Branch);
                                foreach (var s in check_Stock)
                                {
                                    //Update Phieu cu ve status=0
                                    s.Status = false;
                                    s.ModifiedAt = DateTime.Now;
                                    s.LastSync = lastSync;
                                }
                                //Tao thanh phieu moi
                                Stock(item, lastSync);

                                _context.SaveChanges();
                                RecordSuccess++;//Tang so ban ghi luu thanh cong
                            }
                        }//end try
                        catch (Exception axx)
                        {
                            mess_id += item.ProductCode.ToString() + ",";//Luu Id bi loi
                            mess_err += "Balance Stock contains Product Id: " + item.ProductId.ToString() + " - " + GetExceptionDetail(axx) + " ; ";//Luu thong bao
                            returnStock.Add(item);//Add ban ghi loi vao danh sach
                            continue;
                        }
                    }
                    //End Foreach
                }
                //end null 
            }
            //end catch
            catch (Exception ex)
            {
                mess_err += GetExceptionDetail(ex);
            }
            #endregion
        }
        private void SyncProduct(Protocol protocol, out string mess_err, out string mess_id, out int TotalRecord, out int RecordSuccess, out List<tmpProduct> returnProduct, string lastSync)
        {
            mess_err = ""; mess_id = ""; TotalRecord = 0; RecordSuccess = 0;
            returnProduct = new List<tmpProduct>();

            #region Product
            try
            {
                string input_err = "";
                var xdata = ((JArray)protocol.protocol_data.lstprd).ToObject<List<tmpProduct>>();
                List<string> list_product_add = new List<string>();
                List<string> list_product_update = new List<string>();

                List<int> list_product_group_add = new List<int>();
                List<int> list_product_group_update = new List<int>();

                //List chua nhung item da xu ly, de tranh xu ly lai vao cac lan sau
                List<string> list_product_updated = new List<string>();
                List<int> list_product_group_updated = new List<int>();

                #region GetDataAPI
                //Get Id data from API
                foreach (var item in xdata)
                {
                    if (item.IDs != 0)
                    {
                        if (!list_product_add.Contains(item.Code))
                        {
                            if (_context.Products.Where(x => x.IDs == item.IDs && x.Type==item.Type).Count() == 0)
                                list_product_add.Add(item.Code);
                            else
                            if (!list_product_update.Contains(item.Code))
                                list_product_update.Add(item.Code);
                        }

                        if (item.small_group != null)
                        {
                            if (!list_product_group_add.Contains(item.small_group.IDs))
                            {
                                if (_context.ProductGroups.Where(x => x.IDs == item.small_group.IDs).Count() == 0)
                                    list_product_group_add.Add(item.small_group.IDs);
                                else
                                if (!list_product_group_update.Contains(item.small_group.IDs))
                                    list_product_group_update.Add(item.small_group.IDs);
                            }
                        }
                        if (item.middle_group != null)
                        {
                            if (!list_product_group_add.Contains(item.middle_group.IDs))
                            {
                                if (_context.ProductGroups.Where(x => x.IDs == item.middle_group.IDs).Count() == 0)
                                    list_product_group_add.Add(item.middle_group.IDs);
                                else
                                if (!list_product_group_update.Contains(item.middle_group.IDs))
                                    list_product_group_update.Add(item.middle_group.IDs);
                            }
                        }
                        if (item.big_group != null)
                        {
                            if (!list_product_group_add.Contains(item.big_group.IDs))
                            {
                                if (_context.ProductGroups.Where(x => x.IDs == item.big_group.IDs).Count() == 0)
                                    list_product_group_add.Add(item.big_group.IDs);
                                else
                                if (!list_product_group_update.Contains(item.big_group.IDs))
                                    list_product_group_update.Add(item.big_group.IDs);
                            }
                        }
                    }
                }
                #endregion

                if (xdata != null)
                {
                    foreach (var item in xdata)
                    {
                        TotalRecord++;//Tang so ban ghi nhan duoc len
                        input_err = "";
                        try
                        {
                            _context = new ApplicationDbContext();

                            if (item.IDs == 0)
                                input_err = "Id Product is null";
                            if (Convert.ToString(item.Code) == "")
                                input_err = "Code Product is null";
                            if (Convert.ToString(item.Name) == "")
                                input_err = "Name Product is null";
                            if (Convert.ToString(item.Unit) == "")
                                input_err = "Unit Product is null";
                            if (Convert.ToString(item.Type) == "")
                                input_err = "Type Product is null";
                            if (Convert.ToString(item.small_group.IDs) == "")
                                input_err = "small_group is null";
                            if (Convert.ToString(item.middle_group.IDs) == "")
                                input_err = "middle_group is null";
                            if (Convert.ToString(item.big_group.IDs) == "")
                                input_err = "big_group is null";

                            if (input_err != "")
                                throw new Exception(input_err);
                            else
                            {
                                if (list_product_add.Contains(item.Code))//Neu nam trong danh sach Insert
                                {
                                    #region Product
                                    var prd = new Product();
                                    prd.IDs = item.IDs;
                                    prd.Code = item.Code;
                                    prd.Name = item.Name;
                                    prd.Unit = item.Unit;
                                    prd.Shape = item.Shape;
                                    prd.Size = item.Size;
                                    prd.Color = item.Color;
                                    prd.Pearl = item.Pearl;
                                    prd.PearlWeight = item.PearlWeight;
                                    prd.MetaType = item.MetaType;
                                    prd.MetalWeight = item.MetalWeight;
                                    prd.Diamond = item.Diamond;
                                    prd.DiamondWeight = item.DiamondWeight;
                                    prd.GemStoneType = item.GemStoneType;
                                    prd.GemStoneWeight = item.GemStoneWeight;
                                    prd.TotalWeight = item.TotalWeight;
                                    prd.Type = item.Type;
                                    if (item.small_group != null)
                                    {
                                        prd.ProductGroupId1 = item.small_group.IDs;
                                        prd.ProductGroup1Name = item.small_group.Name;
                                    }
                                    else
                                    {
                                        input_err = "Product contains small_group is null";
                                        throw new Exception(input_err);
                                    }
                                    if (item.middle_group != null)
                                    {
                                        prd.ProductGroupId2 = item.middle_group.IDs;
                                        prd.ProductGroup2Name = item.middle_group.Name;
                                    }
                                    else
                                    {
                                        input_err = "Product contains middle_group is null";
                                        throw new Exception(input_err);
                                    }
                                    if (item.big_group != null)
                                    {
                                        prd.ProductGroupId3 = item.big_group.IDs;
                                        prd.ProductGroup3Name = item.big_group.Name;
                                    }
                                    else
                                    {
                                        input_err = "Product contains big_group is null";
                                        throw new Exception(input_err);
                                    }
                                    prd.LastSync = lastSync;
                                    prd.CreatedAt = DateTime.Now;
                                    _context.Products.Add(prd);
                                    list_product_add.Remove(item.Code);

                                    //Add Product Group
                                    ProductGroup(item.small_group, item.middle_group, item.big_group, list_product_group_add, list_product_group_update, list_product_group_updated, lastSync);

                                    _context.SaveChanges();



                                    #endregion
                                }
                                else
                                {
                                    if (list_product_update.Contains(item.Code))//Neu nam trong danh sach Update
                                    {
                                        #region Product
                                        var prd = _context.Products.Where(x => x.IDs == item.IDs && x.Type == item.Type).FirstOrDefault();
                                        if (prd != null && !list_product_updated.Contains(item.Code))
                                        {
                                            prd.Code = item.Code;
                                            prd.Name = item.Name;
                                            prd.Unit = item.Unit;
                                            prd.Shape = item.Shape;
                                            prd.Size = item.Size;
                                            prd.Color = item.Color;
                                            prd.Pearl = item.Pearl;
                                            prd.PearlWeight = item.PearlWeight;
                                            prd.MetaType = item.MetaType;
                                            prd.MetalWeight = item.MetalWeight;
                                            prd.Diamond = item.Diamond;
                                            prd.DiamondWeight = item.DiamondWeight;
                                            prd.GemStoneType = item.GemStoneType;
                                            prd.GemStoneWeight = item.GemStoneWeight;
                                            prd.TotalWeight = item.TotalWeight;
                                            prd.Type = item.Type;
                                            if (item.small_group != null)
                                            {
                                                prd.ProductGroupId1 = item.small_group.IDs;
                                                prd.ProductGroup1Name = item.small_group.Name;
                                            }
                                            else
                                            {
                                                input_err = "Product contains small_group is null";
                                                throw new Exception(input_err);
                                            }
                                            if (item.middle_group != null)
                                            {
                                                prd.ProductGroupId2 = item.middle_group.IDs;
                                                prd.ProductGroup2Name = item.middle_group.Name;
                                            }
                                            else
                                            {
                                                input_err = "Product contains middle_group is null";
                                                throw new Exception(input_err);
                                            }
                                            if (item.big_group != null)
                                            {
                                                prd.ProductGroupId3 = item.big_group.IDs;
                                                prd.ProductGroup3Name = item.big_group.Name;
                                            }
                                            else
                                            {
                                                input_err = "Product contains big_group is null";
                                                throw new Exception(input_err);
                                            }
                                            prd.LastSync = lastSync;
                                            prd.ModifiedAt = DateTime.Now;
                                            list_product_updated.Add(item.Code);

                                            //Add Product Group
                                            ProductGroup(item.small_group, item.middle_group, item.big_group, list_product_group_add, list_product_group_update, list_product_group_updated, lastSync);

                                            _context.SaveChanges();

                                        }
                                        #endregion
                                    }
                                }
                                RecordSuccess++;//Tang so ban ghi luu thanh cong
                            }
                        }//end try
                        catch (Exception ex)
                        {
                            mess_id += item.IDs.ToString() + ",";//Luu Id bi loi
                            mess_err += "Product Code: " + item.Code.ToString() + " - " + GetExceptionDetail(ex) + " ; ";//Luu thong bao
                            returnProduct.Add(item);//Add ban ghi loi vao danh sach
                            continue;
                        }
                    }
                    //End Foreach
                }
                //end null 
            }
            //end catch
            catch (Exception exc)
            {
                mess_err += GetExceptionDetail(exc);
            }
            #endregion
        }
        private void SyncCoffer(Protocol protocol, out string mess_err, out string mess_id, out int TotalRecord, out int RecordSuccess, out List<tmpCoffer> returnCoffer, string lastSync)
        {
            mess_err = ""; mess_id = ""; TotalRecord = 0; RecordSuccess = 0;
            returnCoffer = new List<tmpCoffer>();

            #region Coffer
            try
            {
                string input_err = "";
                var xdata = ((JArray)protocol.protocol_data.coffer_list).ToObject<List<tmpCoffer>>();
                List<int> list_coffer_add = new List<int>();
                List<int> list_coffer_update = new List<int>();
                var list_branch = new Dictionary<int, string>();
                #region GetDataAPI
                //Get Id data from API
                foreach (var item in xdata)
                {
                    if (item.IDs != 0)
                    {
                        if (!list_coffer_add.Contains(item.IDs))
                        {
                            if (_context.Coffers.Where(x => x.IDs == item.IDs).Count() == 0)
                                list_coffer_add.Add(item.IDs);
                            else
                            if (!list_coffer_update.Contains(item.IDs))
                                list_coffer_update.Add(item.IDs);
                        }
                    }
                }
                #endregion

                if (xdata != null)
                {
                    foreach (var item in xdata)
                    {
                        TotalRecord++;//Tang so ban ghi nhan duoc len
                        input_err = "";
                        try
                        {
                            _context = new ApplicationDbContext();

                            if (item.BranchId == 0)
                                input_err = "Id Branch is null";
                            if (Convert.ToString(item.Type) == "")
                                input_err = "Id Type is null";

                            if (input_err != "")
                                throw new Exception(input_err);
                            else
                            {
                                if (list_coffer_add.Contains(item.IDs))//Neu nam trong danh sach Insert
                                {
                                    #region Coffer
                                    var cf = new Coffer();
                                    cf.IDs = item.IDs;
                                    cf.Name = item.Name;
                                    cf.BranchId = item.BranchId;
                                    try
                                    {
                                        if (list_branch.Any(v => v.Key == item.BranchId))
                                            cf.BranchCode = list_branch.Where(c => c.Key == item.BranchId).FirstOrDefault().Value;
                                        else
                                        {
                                            var _code= _context.Branchs.Where(n => n.Id == item.BranchId).FirstOrDefault().BranchCode;
                                            cf.BranchCode = _code;
                                            list_branch.Add(item.BranchId, _code);
                                        }
                                    }
                                    catch { continue; }

                                    cf.Type = item.Type;
                                    cf.LastSync = lastSync;
                                    cf.CreatedAt = DateTime.Now;
                                    _context.Coffers.Add(cf);
                                    list_coffer_add.Remove(item.IDs);

                                    _context.SaveChanges();
                                    #endregion
                                }
                                else
                                {
                                    if (list_coffer_update.Contains(item.IDs))//Neu nam trong danh sach Update
                                    {
                                        #region Coffer
                                        var cfu = _context.Coffers.Where(x => x.IDs == item.IDs).FirstOrDefault();
                                        if (cfu != null && list_coffer_update.Contains(item.IDs))
                                        {
                                            if (cfu.Name != item.Name || cfu.Type != item.Type || cfu.BranchId != item.BranchId)
                                            {
                                                cfu.BranchId = item.BranchId;
                                                cfu.Name = item.Name;
                                                cfu.Type = item.Type;
                                                try
                                                {
                                                    if (list_branch.Any(v => v.Key == item.BranchId))
                                                        cfu.BranchCode = list_branch.Where(c => c.Key == item.BranchId).FirstOrDefault().Value;
                                                    else
                                                    {
                                                        var _code = _context.Branchs.Where(n => n.Id == item.BranchId).FirstOrDefault().BranchCode;
                                                        cfu.BranchCode = _code;
                                                        list_branch.Add(item.BranchId, _code);
                                                    }
                                                }
                                                catch { continue; }

                                                cfu.LastSync = lastSync;
                                                cfu.ModifiedAt = DateTime.Now;
                                                _context.SaveChanges();
                                            }
                                            list_coffer_update.Remove(item.IDs);

                                        }
                                        #endregion
                                    }
                                }
                                RecordSuccess++;//Tang so ban ghi luu thanh cong
                            }
                        }//end try
                        catch (Exception ex)
                        {
                            mess_id += item.IDs.ToString() + ",";//Luu Id bi loi
                            mess_err += "Coffer ID: " + item.IDs.ToString() + " - " + GetExceptionDetail(ex) + " ; ";//Luu thong bao
                            returnCoffer.Add(item);//Add ban ghi loi vao danh sach
                            continue;
                        }
                    }

                    //End Foreach
                }
                //end null 
            }
            //end catch
            catch (Exception exc)
            {
                mess_err += GetExceptionDetail(exc);
            }
            #endregion
        }
        private void SyncReceipt(Protocol protocol, out string mess_err, out string mess_id, out int TotalRecord, out int RecordSuccess, out List<tmpReceipt> returnReceipt, string lastSync)
        {
            mess_err = ""; mess_id = ""; TotalRecord = 0; RecordSuccess = 0;
            returnReceipt = new List<tmpReceipt>();

            #region Receipt
            try
            {
                var xdata = ((JArray)protocol.protocol_data.lsreceipt).ToObject<List<tmpReceipt>>();
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
                List<int> list_bank_account = new List<int>();
                List<int> list_payee = new List<int>();

                //List chua nhung item da xu ly, de tranh xu ly lai vao cac lan sau
                List<int> list_customer_update = new List<int>();
                List<int> list_location_updated = new List<int>();
                List<int> list_sales_updated = new List<int>();
                List<int> list_product_updated = new List<int>();
                List<int> list_coffer_updated = new List<int>();
                List<int> list_country_updated = new List<int>();
                List<int> list_city_updated = new List<int>();
                List<int> list_voucher_updated = new List<int>();
                List<int> list_bank_account_updated = new List<int>();

                #region GetDataAPI
                //Get Id data from API
                foreach (var item in xdata)
                {
                    if (item.Status == 1)//Neu status = 1
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
                    if (item.checkin != null)
                    {
                        if (item.checkin.payee_list != null)
                        {
                            foreach (var r in item.checkin.payee_list)
                            {
                                if (!list_payee.Contains(r.PayeeId))
                                    if (_context.Payes.Where(x => x.IDs == r.PayeeId).Count() == 0)
                                        list_payee.Add(r.PayeeId);
                            }
                        }
                    }
                    if (item.payment != null)
                    {
                        if (item.payment.card_payment_list != null)
                        {
                            foreach (var card in item.payment.card_payment_list)
                            {
                                if (card.BankAccount != null)
                                {
                                    if (card.BankAccount.IDs != 0)
                                    {
                                        if (!list_bank_account.Contains(card.BankAccount.IDs))
                                            if (_context.BankAccounts.Where(x => x.IDs == card.BankAccount.IDs).Count() == 0)
                                                list_bank_account.Add(card.BankAccount.IDs);
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                if (xdata != null)
                {
                    foreach (var item in xdata)
                    {
                        TotalRecord++;//Tang so ban ghi nhan duoc len
                        try
                        {
                            _context = new ApplicationDbContext();
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
                                    bill.CreatedAt = DateTime.Now;

                                    if (item.checkin != null)
                                    {
                                        if (item.checkin.IDs != 0)
                                            bill.CheckinId = item.checkin.IDs;

                                        if (item.checkin.payee_list != null)
                                        {
                                            foreach (var rp in item.checkin.payee_list)
                                            {
                                                var rpa = new ReceiptPayee();
                                                rpa.PayeeId = rp.PayeeId;
                                                rpa.Commission = rp.Commission;
                                                rpa.Money = rp.Money;
                                                rpa.PayeeType = rp.PayeeType;
                                                rpa.ReceiptId = item.IDs;
                                                rpa.CheckInId = item.checkin.IDs;
                                                rpa.LastSync = lastSync;
                                                rpa.CreatedAt = DateTime.Now;
                                                _context.ReceiptPayees.Add(rpa);

                                                Payee(new Models.Payee { Name = rp.PayeeName, Commission = rp.Commission, Phone = "", IDs = rp.PayeeId }, list_payee, lastSync);
                                            }

                                        }
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
                                                else throw new Exception("CustomerId " + item.customer.IDs.ToString() + " contain Country Id is NULL");
                                            }
                                        }
                                        else throw new Exception("Customer Id is NULL");
                                    }
                                    #endregion

                                    #region Salemans
                                    if (item.lst_sale_staff != null)
                                    {
                                        var del_bill_sale = _context.Bill_Sales
                                            .Where(x => x.ReceiptId == item.IDs);
                                        _context.Bill_Sales.RemoveRange(del_bill_sale);

                                        foreach (var s in item.lst_sale_staff)
                                        {
                                            if (s.SalemanId != 0)
                                            {
                                                Saleman(s, list_sales, list_sales_updated, lastSync);

                                                var bill_sale = new ReceiptSalesStaff();
                                                bill_sale.ReceiptId = item.IDs;
                                                bill_sale.SalemanId = s.SalemanId;
                                                bill_sale.Percent = s.Percent;
                                                bill_sale.Money = s.Money;
                                                bill_sale.Status = s.Status;
                                                bill_sale.LastSync = lastSync;
                                                bill_sale.CreatedAt = DateTime.Now;
                                                _context.Bill_Sales.Add(bill_sale);

                                            }
                                        }

                                    }
                                    #endregion
                                    string branchCode = "";
                                    #region Branch
                                    if (item.branch != null)
                                    {
                                        if (item.branch.IDs != 0)
                                        {
                                            //Bill_Branch(item, list_location, list_location_updated, lastSync);
                                            bill.BranchId = item.branch.IDs;
                                            bill.BranchCode = item.branch.BranchCode;
                                            branchCode = item.branch.BranchCode;
                                        }
                                        else throw new Exception("Branch Id is NULL");
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
                                                //Product(s, list_product, list_product_updated, lastSync);

                                                var receipt_detail = new ReceiptDetail();
                                                receipt_detail.ReceiptId = item.IDs;
                                                receipt_detail.ProductId = s.Id;
                                                receipt_detail.ProductCode = s.ProductCode;
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
                                                receipt_detail.CreatedAt = DateTime.Now;


                                                if (s.coffer != null)
                                                {
                                                    if (s.coffer.IDs != 0)
                                                    {
                                                        receipt_detail.CofferId = s.coffer.IDs;
                                                        Coffer(s.coffer, list_coffer, list_coffer_updated, lastSync, branchCode);
                                                    }
                                                    else throw new Exception("ProductId " + s.Id.ToString() + "contain Coffer Id is NULL");
                                                }
                                                _context.ReceiptDetails.Add(receipt_detail);
                                            }
                                            else throw new Exception("Product Id is NULL");
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
                                                receipt_discount.VoucherId = s.VoucherId;
                                                receipt_discount.Percent = s.Percent;
                                                receipt_discount.DiscountType = s.DiscountType;
                                                receipt_discount.MoneyDiscount = s.MoneyDiscount;
                                                receipt_discount.Status = s.Status;
                                                receipt_discount.LastSync = lastSync;
                                                receipt_discount.CreatedAt = DateTime.Now;
                                                _context.ReceiptDiscounts.Add(receipt_discount);

                                            }
                                        }

                                    }
                                    #endregion

                                   

                                    var del_payment = _context.CardPayments.Where(x => x.ReceiptId == item.IDs);
                                    _context.CardPayments.RemoveRange(del_payment);

                                    #region Payment
                                    if (item.payment != null)
                                    {
                                        if (item.payment.card_payment_list != null)
                                        {
                                            foreach (var card in item.payment.card_payment_list)
                                            {
                                                var card_payment = new ReceiptPayment();
                                                card_payment.ReceiptId = item.IDs;
                                                card_payment.BankCardType = card.BankCardType;
                                                card_payment.Amount = card.Amount;
                                                card_payment.Total = card.Total;
                                                card_payment.PaymentType = 2;
                                                card_payment.CurrencyCode = card.CurrencyCode;
                                                card_payment.ExchangeRate = card.ExchangeRate;
                                                if (card.BankAccount != null)
                                                {
                                                    if (card.BankAccount.IDs != 0)
                                                    {
                                                        card_payment.BankId = card.BankAccount.IDs;
                                                        BankAccount(card, list_bank_account, list_bank_account_updated, lastSync);
                                                    }
                                                }
                                                card_payment.Status = card.Status;
                                                card_payment.IsDeposit = false;
                                                card_payment.LastSync = lastSync;
                                                card_payment.CreatedAt = DateTime.Now;

                                                _context.CardPayments.Add(card_payment);
                                            }
                                        }

                                        if (item.payment.cash_payment_list != null)
                                        {
                                            foreach (var cash in item.payment.cash_payment_list)
                                            {
                                                var cash_payment = new ReceiptPayment();
                                                cash_payment.ReceiptId = item.IDs;
                                                cash_payment.Amount = cash.Amount;
                                                cash_payment.Total = cash.Total;
                                                cash_payment.PaymentType = 1;
                                                cash_payment.CurrencyCode = cash.CurrencyCode;
                                                cash_payment.ExchangeRate = cash.ExchangeRate;
                                                cash_payment.Status = cash.Status;
                                                cash_payment.IsDeposit = false;
                                                cash_payment.LastSync = lastSync;
                                                cash_payment.CreatedAt = DateTime.Now;

                                                _context.CardPayments.Add(cash_payment);
                                            }
                                        }
                                    }
                                    #endregion

                                    #region PaymentDeposit
                                    if (item.paymentDeposit != null)
                                    {

                                        if (item.paymentDeposit.card_payment_list != null)
                                        {
                                            foreach (var card in item.paymentDeposit.card_payment_list)
                                            {
                                                if (card.DateDeposit != null)
                                                {
                                                    var card_payment = new ReceiptPayment();
                                                    card_payment.ReceiptId = item.IDs;
                                                    card_payment.BankCardType = card.BankCardType;
                                                    card_payment.Amount = card.Amount;
                                                    card_payment.Total = card.Total;
                                                    card_payment.PaymentType = 2;
                                                    card_payment.DateDeposit = card.DateDeposit;
                                                    card_payment.CurrencyCode = card.CurrencyCode;
                                                    card_payment.ExchangeRate = card.ExchangeRate;
                                                    if (card.BankAccount != null)
                                                    {
                                                        if (card.BankAccount.IDs != 0)
                                                            card_payment.BankId = card.BankId;
                                                    }
                                                    card_payment.Status = card.Status;
                                                    card_payment.IsDeposit = true;
                                                    card_payment.LastSync = lastSync;
                                                    card_payment.CreatedAt = DateTime.Now;

                                                    _context.CardPayments.Add(card_payment);
                                                }
                                                else throw new Exception("Payment Deposit of receipt Id " + item.IDs + " contain Create_date is NULL");
                                            }
                                        }

                                        if (item.paymentDeposit.cash_payment_list != null)
                                        {
                                            foreach (var cash in item.paymentDeposit.cash_payment_list)
                                            {
                                                if (cash.DateDeposit != null)
                                                {
                                                    var cash_payment = new ReceiptPayment();
                                                    cash_payment.ReceiptId = item.IDs;
                                                    cash_payment.Amount = cash.Amount;
                                                    cash_payment.Total = cash.Total;
                                                    cash_payment.PaymentType = 1;
                                                    cash_payment.DateDeposit = cash.DateDeposit;
                                                    cash_payment.CurrencyCode = cash.CurrencyCode;
                                                    cash_payment.ExchangeRate = cash.ExchangeRate;
                                                    cash_payment.Status = cash.Status;
                                                    cash_payment.IsDeposit = true;
                                                    cash_payment.LastSync = lastSync;
                                                    cash_payment.CreatedAt = DateTime.Now;

                                                    _context.CardPayments.Add(cash_payment);
                                                }
                                                else throw new Exception("Payment Deposit of receipt Id " + item.IDs + " contain Create_date is NULL");
                                            }
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
                                    var bill_remove = _context.Receipts.Where(x => x.IDs == item.IDs && x.Status == 1);
                                    foreach (var item_d in bill_remove)
                                    {
                                        item_d.Status = item.Status;
                                        item_d.LastSync = lastSync;
                                        item_d.ModifiedAt = DateTime.Now;
                                    }
                                    
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
                                        receipt_update.ModifiedAt = DateTime.Now;

                                        if (item.checkin != null)
                                        {
                                            if (item.checkin.IDs != 0)
                                                receipt_update.CheckinId = item.checkin.IDs;

                                            if (item.checkin.payee_list != null)
                                            {
                                                var del_receipt_payee = _context.ReceiptPayees
                                               .Where(x => x.ReceiptId == item.IDs && x.CheckInId == item.checkin.IDs);
                                                _context.ReceiptPayees.RemoveRange(del_receipt_payee);

                                                foreach (var rp in item.checkin.payee_list)
                                                {
                                                    var rpa = new ReceiptPayee();
                                                    rpa.PayeeId = rp.PayeeId;
                                                    rpa.CheckInId = item.checkin.IDs;
                                                    rpa.ReceiptId = item.IDs;
                                                    rpa.Commission = rp.Commission;
                                                    rpa.Money = rp.Money;
                                                    rpa.PayeeType = rp.PayeeType;
                                                    rpa.LastSync = lastSync;
                                                    rpa.ModifiedAt = DateTime.Now;
                                                    _context.ReceiptPayees.Add(rpa);
                                                }

                                            }
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
                                                    else throw new Exception("CustomerId " + item.customer.IDs.ToString() + " contain Country Id is NULL");
                                                }
                                            }
                                            else throw new Exception("Customer Id is NULL");
                                        }
                                        #endregion

                                        #region Salemans
                                        if (item.lst_sale_staff != null)
                                        {
                                            var del_bill_sale = _context.Bill_Sales
                                                .Where(x => x.ReceiptId == item.IDs);
                                            _context.Bill_Sales.RemoveRange(del_bill_sale);

                                            foreach (var s in item.lst_sale_staff)
                                            {
                                                if (s.SalemanId != 0)
                                                {
                                                    Saleman(s, list_sales, list_sales_updated, lastSync);

                                                    var bill_sale = new ReceiptSalesStaff();
                                                    bill_sale.ReceiptId = item.IDs;
                                                    bill_sale.SalemanId = s.SalemanId;
                                                    bill_sale.Percent = s.Percent;
                                                    bill_sale.Money = s.Money;
                                                    bill_sale.Status = s.Status;
                                                    bill_sale.LastSync = lastSync;
                                                    bill_sale.CreatedAt = DateTime.Now;
                                                    _context.Bill_Sales.Add(bill_sale);
                                                }
                                            }

                                        }
                                        #endregion
                                        string branchCode = "";
                                        #region Branch
                                        if (item.branch != null)
                                        {
                                            if (item.branch.IDs != 0)
                                            {
                                                //Bill_Branch(item, list_location, list_location_updated, lastSync);
                                                receipt_update.BranchId = item.branch.IDs;
                                                receipt_update.BranchCode = item.branch.BranchCode;
                                                branchCode = item.branch.BranchCode;
                                            }
                                            else throw new Exception("Branch Id is NULL");
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
                                                    //Product(s, list_product, list_product_updated, lastSync);

                                                    var receipt_detail = new ReceiptDetail();
                                                    receipt_detail.ReceiptId = item.IDs;
                                                    receipt_detail.ProductId = s.Id;
                                                    receipt_detail.ProductCode = s.ProductCode;
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
                                                    receipt_detail.CreatedAt = DateTime.Now;


                                                    if (s.coffer != null)
                                                    {
                                                        if (s.coffer.IDs != 0)
                                                        {
                                                            receipt_detail.CofferId = s.coffer.IDs;
                                                            Coffer(s.coffer, list_coffer, list_coffer_updated, lastSync, branchCode);
                                                        }
                                                        else throw new Exception("ProductId " + s.Id.ToString() + " contains Coffer Id is NULL");
                                                    }
                                                    _context.ReceiptDetails.Add(receipt_detail);
                                                }
                                                else throw new Exception("Product Id is NULL");
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
                                                    receipt_discount.VoucherId = s.VoucherId;
                                                    receipt_discount.Percent = s.Percent;
                                                    receipt_discount.DiscountType = s.DiscountType;
                                                    receipt_discount.MoneyDiscount = s.MoneyDiscount;
                                                    receipt_discount.Status = s.Status;
                                                    receipt_discount.LastSync = lastSync;
                                                    receipt_discount.CreatedAt = DateTime.Now;
                                                    _context.ReceiptDiscounts.Add(receipt_discount);

                                                }
                                            }

                                        }
                                        #endregion

                                        

                                        var del_payment = _context.CardPayments.Where(x => x.ReceiptId == item.IDs);
                                        _context.CardPayments.RemoveRange(del_payment);

                                        #region Payment
                                        if (item.payment != null)
                                        {
                                            if (item.payment.card_payment_list != null)
                                            {
                                                foreach (var card in item.payment.card_payment_list)
                                                {
                                                    var card_payment = new ReceiptPayment();
                                                    card_payment.ReceiptId = item.IDs;
                                                    card_payment.BankCardType = card.BankCardType;
                                                    card_payment.Amount = card.Amount;
                                                    card_payment.Total = card.Total;
                                                    card_payment.PaymentType = 2;
                                                    card_payment.CurrencyCode = card.CurrencyCode;
                                                    card_payment.ExchangeRate = card.ExchangeRate;
                                                    if (card.BankAccount != null)
                                                    {
                                                        if (card.BankAccount.IDs != 0)
                                                        {
                                                            card_payment.BankId = card.BankAccount.IDs;
                                                            BankAccount(card, list_bank_account, list_bank_account_updated, lastSync);
                                                        }
                                                    }

                                                    card_payment.Status = card.Status;
                                                    card_payment.IsDeposit = false;
                                                    card_payment.LastSync = lastSync;
                                                    card_payment.CreatedAt = DateTime.Now;

                                                    _context.CardPayments.Add(card_payment);
                                                }
                                            }

                                            if (item.payment.cash_payment_list != null)
                                            {
                                                foreach (var cash in item.payment.cash_payment_list)
                                                {
                                                    var cash_payment = new ReceiptPayment();
                                                    cash_payment.ReceiptId = item.IDs;
                                                    cash_payment.Amount = cash.Amount;
                                                    cash_payment.Total = cash.Total;
                                                    cash_payment.PaymentType = 1;
                                                    cash_payment.CurrencyCode = cash.CurrencyCode;
                                                    cash_payment.ExchangeRate = cash.ExchangeRate;
                                                    cash_payment.Status = cash.Status;
                                                    cash_payment.IsDeposit = false;
                                                    cash_payment.LastSync = lastSync;
                                                    cash_payment.CreatedAt = DateTime.Now;

                                                    _context.CardPayments.Add(cash_payment);
                                                }
                                            }
                                        }
                                        #endregion

                                        #region PaymentDeposit
                                        if (item.paymentDeposit != null)
                                        {

                                            if (item.paymentDeposit.card_payment_list != null)
                                            {
                                                foreach (var card in item.paymentDeposit.card_payment_list)
                                                {
                                                    if (card.DateDeposit != null)
                                                    {
                                                        var card_payment = new ReceiptPayment();
                                                        card_payment.ReceiptId = item.IDs;
                                                        card_payment.BankCardType = card.BankCardType;
                                                        card_payment.Amount = card.Amount;
                                                        card_payment.Total = card.Total;
                                                        card_payment.PaymentType = 2;
                                                        card_payment.DateDeposit = card.DateDeposit;
                                                        card_payment.CurrencyCode = card.CurrencyCode;
                                                        card_payment.ExchangeRate = card.ExchangeRate;
                                                        if (card.BankAccount != null)
                                                        {
                                                            if (card.BankAccount.IDs != 0)
                                                                card_payment.BankId = card.BankId;
                                                        }
                                                        card_payment.Status = card.Status;
                                                        card_payment.IsDeposit = true;
                                                        card_payment.LastSync = lastSync;
                                                        card_payment.CreatedAt = DateTime.Now;

                                                        _context.CardPayments.Add(card_payment);
                                                    }
                                                    else throw new Exception("Payment Deposit of receipt Id " + item.IDs + " contain Create_date is NULL");
                                                }
                                            }

                                            if (item.paymentDeposit.cash_payment_list != null)
                                            {
                                                foreach (var cash in item.paymentDeposit.cash_payment_list)
                                                {
                                                    if (cash.DateDeposit != null)
                                                    {
                                                        var cash_payment = new ReceiptPayment();
                                                        cash_payment.ReceiptId = item.IDs;
                                                        cash_payment.Amount = cash.Amount;
                                                        cash_payment.Total = cash.Total;
                                                        cash_payment.PaymentType = 1;
                                                        cash_payment.DateDeposit = cash.DateDeposit;
                                                        cash_payment.CurrencyCode = cash.CurrencyCode;
                                                        cash_payment.ExchangeRate = cash.ExchangeRate;
                                                        cash_payment.Status = cash.Status;
                                                        cash_payment.IsDeposit = true;
                                                        cash_payment.LastSync = lastSync;
                                                        cash_payment.CreatedAt = DateTime.Now;

                                                        _context.CardPayments.Add(cash_payment);
                                                    }
                                                    else throw new Exception("Payment Deposit of receipt Id " + item.IDs + " contain Create_date is NULL");
                                                }
                                            }
                                        }
                                        #endregion
                                    }
                                }
                            }
                            _context.SaveChanges();
                            RecordSuccess++;//Tang so ban ghi luu thanh cong
                        }//end try
                        catch (Exception ex)
                        {
                            mess_id += item.IDs.ToString() + ",";//Luu Id bi loi
                            mess_err += "Receipt ID: " + item.IDs.ToString() + " - " + GetExceptionDetail(ex) + " ; ";//Luu thong bao
                            returnReceipt.Add(item);//Add ban ghi loi vao danh sach
                            continue;
                        }
                    }
                    //End Foreach
                }
                //end null 
            }
            //end catch
            catch (Exception axx)
            {
                mess_err += GetExceptionDetail(axx);
            }
            #endregion
        }
        private void SyncCheckIn(Protocol protocol, out string mess_err, out string mess_id, out int TotalRecord, out int RecordSuccess, out List<tmpCheckIn> returnCheckin, string lastSync)
        {
            mess_err = ""; mess_id = ""; TotalRecord = 0; RecordSuccess = 0;
            returnCheckin = new List<tmpCheckIn>();

            #region CheckIn
            var xdata = ((JArray)protocol.protocol_data.checkin_list).ToObject<List<tmpCheckIn>>();

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
                List<int> list_contact_fee = new List<int>();

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
                List<int> list_contact_fee_updated = new List<int>();

                #region GetDataAPI
                //Get Id data from API
                foreach (var item in xdata)
                {
                    if (item.Status == 1)//Neu status = 1
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
                    if (item.fee_port_contact != null)
                    {
                        if (!list_contact_fee.Contains(item.fee_port_contact.IDs))
                            if (_context.ContactFeePorts.Where(x => x.IDs == item.fee_port_contact.IDs).Count() == 0)
                                list_contact_fee.Add(item.fee_port_contact.IDs);
                    }
                }
                #endregion

                if (xdata != null)
                {

                    foreach (var item in xdata)
                    {
                        TotalRecord++;//Tang so luong ban ghi nhan duoc
                        try
                        {
                            _context = new ApplicationDbContext();
                            if (list_checkin_add.Contains(item.IDs))//Neu nam trong danh sach Insert
                            {

                                if (!_context.tblCheckIns.Local.Any(x => x.IDs == item.IDs))
                                {
                                    var checkin = new CheckIn();
                                    checkin.IDs = item.IDs;
                                    checkin.CheckInCode = item.CheckInCode;
                                    if (item.CreatedDate != null)
                                        checkin.CreatedDate = item.CreatedDate;
                                    
                                    checkin.Status = item.Status;
                                    checkin.LastSync = lastSync;
                                    checkin.CreatedAt = DateTime.Now;

                                    #region CardType
                                    if (item.card_type != null)
                                    {
                                        if (item.card_type.IDs != 0)
                                        {
                                            CardType(item, list_card_type, lastSync);
                                            checkin.CardTypeId = item.card_type.IDs;
                                        }
                                        else
                                            throw new Exception("CardType Id is NULL");
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
                                        else
                                            throw new Exception("CardMaker Id is NULL");
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
                                        else
                                            throw new Exception("Driver Id is NULL");
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
                                        else throw new Exception("TourGuide Id is NULL");
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
                                        else throw new Exception("Visitor Id is NULL");
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
                                            var checkin_car = new CheckInCar();
                                            checkin_car.CheckInId = item.IDs;
                                            checkin_car.CarNumber = c.Car_Number;
                                            checkin_car.LastSync = lastSync;
                                            checkin_car.CreatedAt = DateTime.Now;

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
                                            else throw new Exception("Payee Id is NULL");

                                            var del_checkin_payee = _context.CheckIn_Payes.Where(x => x.CheckInId == item.IDs);
                                            if (del_checkin_payee != null)
                                                _context.CheckIn_Payes.RemoveRange(del_checkin_payee);

                                            var checkin_payee = new CheckInPayee();
                                            checkin_payee.CheckInId = item.IDs;
                                            checkin_payee.PayeeId = c.IDs;
                                            checkin_payee.LastSync = lastSync;
                                            checkin_payee.CreatedAt = DateTime.Now;

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

                                            var checkin_info = new CheckInDetail();
                                            checkin_info.NumberAdult = i.NumberAdult;
                                            checkin_info.NumberBaby = i.NumberBaby;
                                            checkin_info.NumberChild = i.NumberChild;
                                            if (i.NationalityId != 0)
                                                checkin_info.NationalityId = i.NationalityId;
                                            checkin_info.CheckinId = item.IDs;
                                            checkin_info.LastSync = lastSync;
                                            checkin_info.CreatedAt = DateTime.Now;

                                            _context.CheckIn_Infos.Add(checkin_info);
                                        }
                                    }
                                    #endregion

                                    #region Branch
                                    if (item.location != null)
                                    {
                                        if (item.location.IDs != 0)
                                        {
                                            //Branch(item, list_location, lastSync);
                                            checkin.BranchId = item.location.IDs;
                                            checkin.BranchCode = item.location.BranchCode;
                                        }
                                        else throw new Exception("Branch Id is NULL");
                                    }
                                    #endregion

                                    #region ContactFee
                                    if (item.fee_port_contact != null)
                                    {
                                        if (item.fee_port_contact.IDs != 0)
                                        {
                                            FeePortName(item.fee_port_contact, list_contact_fee, list_contact_fee_updated, lastSync);
                                            checkin.ContactFeePortId = item.fee_port_contact.IDs;
                                            checkin.FeePort = item.fee_port_contact.FeePort;
                                        }
                                        else throw new Exception("Contact Fee Port is NULL");
                                    }
                                    #endregion
                                    _context.tblCheckIns.Add(checkin);
                                }
                            }
                            else
                            {
                                if (list_checkin_remove.Contains(item.IDs))//Neu nam trong danh sach Remove, Update Status ve 0
                                {
                                    var checkin_remove = _context.tblCheckIns.Where(x => x.IDs == item.IDs && x.Status == 1);
                                    foreach (var item_d in checkin_remove)
                                    {
                                        item_d.Status = item.Status;
                                        item_d.LastSync = lastSync;
                                        item_d.ModifiedAt = DateTime.Now;
                                    }
                                   
                                }
                                else
                                {
                                    if (list_checkin_update.Contains(item.IDs))//Neu nam trong danh sach Update
                                    {
                                        var checkin_update = _context.tblCheckIns.Where(x => x.IDs == item.IDs).FirstOrDefault();
                                        checkin_update.Status = item.Status;
                                        checkin_update.CheckInCode = item.CheckInCode;
                                        if (item.CreatedDate != null)
                                            checkin_update.CreatedDate = item.CreatedDate;
                                        else
                                            checkin_update.CreatedDate = null;

                                        
                                        checkin_update.LastSync = lastSync;
                                        checkin_update.ModifiedAt = DateTime.Now;

                                        #region CardType
                                        if (item.card_type != null)
                                        {
                                            if (item.card_type.IDs != 0)
                                            {
                                                CardType(item, list_card_type, lastSync);
                                                checkin_update.CardTypeId = item.card_type.IDs;
                                            }
                                            else throw new Exception("CardType Id is NULL");
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
                                            else throw new Exception("CardMaker Id is NULL");
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
                                            else throw new Exception("Driver Id is NULL");
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
                                            else throw new Exception("TourGuide Id is NULL");
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
                                            else throw new Exception("Visitor Id is NULL");
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
                                                var checkin_car = new CheckInCar();
                                                checkin_car.CheckInId = item.IDs;
                                                checkin_car.CarNumber = c.Car_Number;
                                                checkin_car.LastSync = lastSync;
                                                checkin_car.CreatedAt = DateTime.Now;

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
                                                else throw new Exception("Payee Id is NULL");

                                                var del_checkin_payee = _context.CheckIn_Payes.Where(x => x.CheckInId == item.IDs);
                                                if (del_checkin_payee != null)
                                                    _context.CheckIn_Payes.RemoveRange(del_checkin_payee);

                                                var checkin_payee = new CheckInPayee();
                                                checkin_payee.CheckInId = item.IDs;
                                                checkin_payee.PayeeId = c.IDs;
                                                checkin_payee.LastSync = lastSync;
                                                checkin_payee.CreatedAt = DateTime.Now;

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

                                                var checkin_info = new CheckInDetail();
                                                checkin_info.NumberAdult = i.NumberAdult;
                                                checkin_info.NumberBaby = i.NumberBaby;
                                                checkin_info.NumberChild = i.NumberChild;
                                                if (i.NationalityId != 0)
                                                    checkin_info.NationalityId = i.NationalityId;
                                                checkin_info.CheckinId = item.IDs;
                                                checkin_info.LastSync = lastSync;
                                                checkin_info.CreatedAt = DateTime.Now;

                                                _context.CheckIn_Infos.Add(checkin_info);
                                            }
                                        }
                                        #endregion

                                        #region Branch
                                        if (item.location != null)
                                        {
                                            if (item.location.IDs != 0)
                                            {
                                                //Branch(item, list_location, lastSync);
                                                checkin_update.BranchId = item.location.IDs;
                                                checkin_update.BranchCode = item.location.BranchCode;
                                            }
                                            else throw new Exception("Branch Id is NULL");
                                        }
                                        #endregion
                                        #region ContactFee
                                        if (item.fee_port_contact != null)
                                        {
                                            if (item.fee_port_contact.IDs != 0)
                                            {
                                                FeePortName(item.fee_port_contact, list_contact_fee, list_contact_fee_updated, lastSync);
                                                checkin_update.ContactFeePortId = item.fee_port_contact.IDs;
                                                checkin_update.FeePort = item.fee_port_contact.FeePort;
                                            }
                                            else throw new Exception("Contact Fee Port is NULL");
                                        }
                                        else
                                        {
                                            checkin_update.ContactFeePortId = null;
                                            checkin_update.FeePort = 0;
                                        }
                                        #endregion
                                    }
                                }
                            }
                            _context.SaveChanges();
                            RecordSuccess++;//Tang so ban ghi luu thanh cong
                        }
                        catch (Exception ex)
                        {
                            mess_id += item.IDs.ToString() + ",";//Nhung Id bi loi
                            mess_err += "Checkin ID: " + item.IDs.ToString() + " - " + GetExceptionDetail(ex) + " ; ";//Thong bao loi
                            returnCheckin.Add(item);//Add nhung ban ghi bi loi vao danh sach
                            continue;
                        }
                    }
                    //End Foreach
                }
                //end protocol.checkin_list 
            }

            catch (Exception axx)
            {
                mess_err += GetExceptionDetail(axx);
            }
            #endregion
        }
        private void SyncInventory(Protocol protocol, out string mess_err, out string mess_id, out int TotalRecord, out int RecordSuccess, out List<tmpInventory> returnInventory, string lastSync)
        {
            mess_err = ""; mess_id = ""; TotalRecord = 0; RecordSuccess = 0;
            returnInventory = new List<tmpInventory>();

            #region Inventory
            try
            {
                var xdata = ((JArray)protocol.protocol_data.product_history).ToObject<List<tmpInventory>>();
                List<int> list_inventory_add = new List<int>();
                List<int> list_inventory_update = new List<int>();
                List<int> list_inventoty_remove = new List<int>();
                List<int> list_supplier = new List<int>();
                List<int> list_coffer = new List<int>();


                //List chua nhung item da xu ly, de tranh xu ly lai vao cac lan sau
                List<int> list_supplier_update = new List<int>();
                List<int> list_coffer_updated = new List<int>();


                #region GetDataAPI
                //Get Id data from API
                foreach (var item in xdata)
                {
                    //Neu ko co trong DB cho vao danh sach Insert
                    if (!list_inventory_add.Contains(item.IDs))
                        //if (_context.Inventorys.Where(x => x.IDs == item.IDs).Count() == 0)
                        list_inventory_add.Add(item.IDs);

                    //Neu da ton tai trong DB thi cho vao danh sach Update
                    if (!list_inventory_update.Contains(item.IDs))
                        if (_context.Inventorys.Where(x => x.IDs == item.IDs).Count() > 0)
                            list_inventory_update.Add(item.IDs);

                    if (item.SupplierId != null)
                    {
                        if (!list_supplier.Contains(item.SupplierId.IDs))
                            if (_context.Suppliers.Where(x => x.IDs == item.SupplierId.IDs).Count() == 0)
                                list_supplier.Add(item.SupplierId.IDs);

                    }

                    if (item.StockFrom != null)
                    {
                        if (!list_coffer.Contains(item.StockFrom.IDs))
                            if (_context.Coffers.Where(x => x.IDs == item.StockFrom.IDs).Count() == 0)
                                list_coffer.Add(item.StockFrom.IDs);
                    }

                    if (item.StockTo != null)
                    {
                        if (!list_coffer.Contains(item.StockTo.IDs))
                            if (_context.Coffers.Where(x => x.IDs == item.StockTo.IDs).Count() == 0)
                                list_coffer.Add(item.StockTo.IDs);
                    }
                }
                #endregion

                if (xdata != null)
                {
                    foreach (var item in xdata)
                    {
                        TotalRecord++;//Tang so ban ghi nhan duoc len
                        try
                        {
                            _context = new ApplicationDbContext();
                            if (list_inventory_update.Contains(item.IDs))
                            {
                                var inven_update = _context.Inventorys.Where(x => x.IDs == item.IDs && x.Status == 1);
                                foreach (var item_d in inven_update)
                                {
                                    item_d.Status = 0;
                                    item_d.ModifiedAt = DateTime.Now;
                                    item_d.LastSync = lastSync;
                                }
                               
                            }

                            if (list_inventory_add.Contains(item.IDs))//Neu nam trong danh sach Insert
                            {
                                var inven = new Inventory();
                                inven.IDs = item.IDs;
                                inven.Code = item.Code;
                                inven.ProductId = item.ProductId;
                                inven.ProductCode = item.ProductCode;
                                inven.ProductType = item.ProductType;
                                inven.CreatedDate = item.CreatedDate;
                                inven.Quantity = item.Quantity;
                                inven.Price = item.Price;
                                inven.Amount = item.Amount;
                                inven.Description = item.Description;
                                inven.Type = item.Type;
                                inven.Code_Class = item.Code_Class;
                                inven.LastSync = lastSync;
                                inven.CreatedAt = DateTime.Now;
                                inven.Status = 1;

                                if (item.Store != null)
                                {
                                    if (item.Store.IDs != 0)
                                        inven.StoreId = item.Store.IDs;
                                }

                                #region Supplier
                                if (item.SupplierId != null)
                                {
                                    if (item.SupplierId.IDs != 0)
                                    {
                                        Supplier(item.SupplierId, list_supplier, list_supplier_update, lastSync);
                                        inven.SupplierId = item.SupplierId.IDs;
                                    }

                                }
                                #endregion

                                string branchCode = "";
                                #region Location
                                if (item.Store != null)
                                {
                                    if (item.Store.IDs != 0)
                                    {
                                        inven.StoreId = item.Store.IDs;
                                        var x = _context.Branchs.Where(n => n.IDs == item.Store.IDs);
                                        if(x.Count()>0)
                                            branchCode = x.FirstOrDefault().BranchCode;
                                    }

                                }
                                #endregion

                                #region CofferFrom
                                if (item.StockFrom != null)
                                {
                                    if (item.StockFrom.IDs != 0)
                                    {
                                        inven.StockFromId = item.StockFrom.IDs;
                                        Coffer(item.StockFrom, list_coffer, list_coffer_updated, lastSync, branchCode);
                                    }

                                }
                                #endregion

                                #region CofferTo
                                if (item.StockTo != null)
                                {
                                    if (item.StockTo.IDs != 0)
                                    {
                                        inven.StockToId = item.StockTo.IDs;
                                        Coffer(item.StockFrom, list_coffer, list_coffer_updated, lastSync, branchCode);
                                    }

                                }
                                #endregion
                               

                                _context.Inventorys.Add(inven);

                            }
                            _context.SaveChanges();
                            RecordSuccess++;//Tang so ban ghi luu thanh cong
                        }//end try
                        catch (Exception ex)
                        {
                            mess_id += item.IDs.ToString() + ",";//Luu Id bi loi
                            mess_err += "Inventory ID: " + item.IDs.ToString() + " - " + GetExceptionDetail(ex) + " ; ";//Luu thong bao
                            returnInventory.Add(item);//Add ban ghi loi vao danh sach
                            continue;
                        }
                    }
                    //End Foreach
                }
                //end null 
            }
            //end catch
            catch (Exception axx)
            {
                mess_err += GetExceptionDetail(axx);
            }
            #endregion
        }
        private void SyncStore(Protocol protocol, out string mess_err, out string mess_id, out int TotalRecord, out int RecordSuccess, out List<tmpBranch> returnStore, string lastSync)
        {
            mess_err = ""; mess_id = ""; TotalRecord = 0; RecordSuccess = 0;
            returnStore = new List<tmpBranch>();
            #region Store
            try
            {
                string input_err = "";
                var xdata = ((JArray)protocol.protocol_data.lstBranch).ToObject<List<tmpBranch>>();
                List<int> list_store_add = new List<int>();
                List<int> list_store_update = new List<int>();

                #region GetDataAPI
                //Get Id data from API
                foreach (var item in xdata)
                {
                    if (item.IDs != 0)
                    {
                        if (!list_store_add.Contains(item.IDs))
                        {
                            if (_context.Branchs.Where(x => x.IDs == item.IDs).Count() == 0)
                                list_store_add.Add(item.IDs);
                            else
                            if (!list_store_update.Contains(item.IDs))
                                list_store_update.Add(item.IDs);
                        }
                    }
                }
                #endregion

                if (xdata != null)
                {
                    foreach (var item in xdata)
                    {
                        TotalRecord++;//Tang so ban ghi nhan duoc len
                        input_err = "";
                        try
                        {
                            _context = new ApplicationDbContext();

                            if (item.IDs == 0)
                                input_err = "Id Branch is null";
                            if (Convert.ToString(item.BranchCode) == "")
                                input_err = "Id BranchCode is null";
                            if (Convert.ToString(item.BranchName) == "")
                                input_err = "Id BranchName is null";

                            if (input_err != "")
                                throw new Exception(input_err);
                            else
                            {
                                if (list_store_add.Contains(item.IDs))//Neu nam trong danh sach Insert
                                {
                                    #region Store
                                    var cf = new Branch();
                                    cf.IDs = item.IDs;
                                    cf.BranchName = item.BranchName;
                                    cf.BranchCode = item.BranchCode;
                                    cf.Address = item.Address;
                                    cf.Phone = item.Phone;
                                    cf.LastSync = lastSync;
                                    cf.CreatedAt = DateTime.Now;
                                    if (item.Country != null)
                                    {
                                        cf.CountryId = item.Country.IDs;
                                    }
                                    _context.Branchs.Add(cf);
                                    list_store_add.Remove(item.IDs);

                                    _context.SaveChanges();
                                    #endregion
                                }
                                else
                                {
                                    if (list_store_update.Contains(item.IDs))//Neu nam trong danh sach Update
                                    {
                                        #region Store
                                        var cfu = _context.Branchs.Where(x => x.IDs == item.IDs).FirstOrDefault();
                                        if (cfu != null && list_store_update.Contains(item.IDs))
                                        {
                                            if (cfu.BranchCode != item.BranchCode || cfu.BranchName != item.BranchName)
                                            {
                                                cfu.BranchCode = item.BranchCode;
                                                cfu.BranchName = item.BranchName;
                                                cfu.Address = item.Address;
                                                cfu.Phone = item.Phone;
                                                if (item.Country != null)
                                                {
                                                    cfu.CountryId = item.Country.IDs;
                                                }
                                                else cfu.CountryId = null;

                                                cfu.LastSync = lastSync;
                                                cfu.ModifiedAt = DateTime.Now;
                                                _context.SaveChanges();
                                            }
                                            list_store_update.Remove(item.IDs);

                                        }
                                        #endregion
                                    }
                                }
                                RecordSuccess++;//Tang so ban ghi luu thanh cong
                            }
                        }//end try
                        catch (Exception ex)
                        {
                            mess_id += item.IDs.ToString() + ",";//Luu Id bi loi
                            mess_err += "Branch ID: " + item.IDs.ToString() + " - " + GetExceptionDetail(ex) + " ; ";//Luu thong bao
                            returnStore.Add(item);//Add ban ghi loi vao danh sach
                            continue;
                        }
                    }
                    //End Foreach
                }
                //end null 
            }
            //end catch
            catch (Exception exc)
            {
                mess_err += GetExceptionDetail(exc);
            }
            #endregion
        }
        private string GetExceptionDetail(Exception ax)
        {
            if (ax.InnerException != null)
            {
                if (ax.InnerException.InnerException != null)
                    return ax.InnerException.InnerException.Message;
                else
                    return ax.InnerException.Message;
            }
            else return ax.Message;
        }
    }
}
