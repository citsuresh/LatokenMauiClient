using Latoken.Api.Client.Library;
using Latoken.Api.Client.Library.Commands;
using Latoken.Api.Client.Library.Constants;
using Latoken.Api.Client.Library.Dto.Rest;
using Latoken.Api.Client.Library.LA.Commands;
using Latoken.Api.Client.Library.Utils.Configuration;

namespace LatokenMauiClient
{
    public class LatokenRestClient : IRestClient
    {
        public LatokenRestClient(string publicKey, string privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        private ILARestClient m_restClient;

        public ILARestClient ClientInstance
        {
            get
            {
                if (m_restClient == null)
                {
                    m_restClient = GetLaTokenRestClient();
                }

                return m_restClient;
            }

            private set => m_restClient = value;
        }

        public TradingPlatform TradingPlatform => TradingPlatform.Latoken;

        public string PublicKey { get; private set; }
        public string PrivateKey { get; private set; }


        private ILARestClient? GetLaTokenRestClient()
        {
            //Generate your public and private API keys on this page https://latoken.com/account/apikeys
            ClientCredentials credentials = new ClientCredentials
            {
                ApiKey = PublicKey, //"Your Public API Key",
                ApiSecret = PrivateKey, //"Your Private API Key"
            };

            var latokenRestClient = new LARestClient(credentials, new HttpClient() { BaseAddress = new Uri("https://api.latoken.com") });

            return latokenRestClient;
        }

        public IEnumerable<CurrencyDto> GetCurrencies()
        {
            var currencies = ClientInstance.GetCurrencies().Result;
            return currencies.Select(c => new CurrencyDto { Id = c.Id, Name = c.Name, Symbol = c.Tag });
        }

        //public CurrencyDto GetCurrency(string currencyId)
        //{
        //    var c = ClientInstance.GetCurrency(currencyId).Result;
        //    return new CurrencyDto { Id = c.Id, Name = c.Name, Symbol = c.Tag };
        //}

        public IEnumerable<PairDto> GetAvailablePairs()
        {
            var pairs = ClientInstance.GetAvailablePairs().Result;
            return pairs.Select(c => new PairDto { BaseCurrencyId = c.BaseCurrency, QuoteCurrencyId = c.QuoteCurrency, QuantityDecimals = c.QuantityDecimals, PriceTick = c.PriceTick });
        }

        public IEnumerable<TradingCompetition> GetTradingCompetitions(int page = 0)
        {
            var tradingCompetitions = ClientInstance.GetTradingCompetitions(page).Result.Content;
            return tradingCompetitions;
        }


        public IEnumerable<TradingCompetitionLeaderBoardPosition> GetTradingCompetitionLeaderBoard(string competitionId, int page = 0, int size = 100)
        {
            var leaderboardPositions = ClientInstance.GetTradingCompetitionLeaderBoardPosition(competitionId, page, size).Result.Content;
            return leaderboardPositions;
        }

        public OrderBookDto GetOrderBook(CurrencyDto baseCurrency, CurrencyDto quoteCurrency, int limit)
        {
            var orderBook = new OrderBookDto();
            var laOrderBook = ClientInstance.GetOrderBook(baseCurrency.Id, quoteCurrency.Id, limit);
            if (laOrderBook != null && laOrderBook.Result != null && laOrderBook.Result != null)
            {
                orderBook.Ask = new List<PriceLevelDto>();
                foreach (var ask in laOrderBook.Result.Ask)
                {
                    orderBook.Ask.Add(new PriceLevelDto { IsBuyOrder = false, Price = ask.Price, Quantity = ask.Quantity, Accumulated = ask.Accumulated, Cost = ask.Cost });
                }
                orderBook.Bid = new List<PriceLevelDto>();
                foreach (var bid in laOrderBook.Result.Bid)
                {
                    orderBook.Bid.Add(new PriceLevelDto { IsBuyOrder = true, Price = bid.Price, Quantity = bid.Quantity, Accumulated = bid.Accumulated, Cost = bid.Cost });
                }
                orderBook.TotalBid = laOrderBook.Result.TotalBid;
                orderBook.TotalAsk = laOrderBook.Result.TotalAsk;
            }
            return orderBook;
        }
        public FeeScheme GetFeeSchemeForPair(string baseCurrency, string quoteCurrency)
        {
            return ClientInstance.GetFeeSchemeForPair(baseCurrency, quoteCurrency).Result;
        }
        public IEnumerable<TickerDto> GetTickers()
        {
            var tickers = ClientInstance.GetTickers().Result;
            return tickers.Select(t => new TickerDto
            {
                Symbol = t.Symbol,
                BaseCurrency = t.BaseCurrency,
                QuoteCurrency = t.QuoteCurrency,
                BaseVolume24h = t.Volume24h.ToString(),
                Volume7d = t.Volume7d,
                Change24h = t.Change24h.ToString(),
                Change7d = t.Change7d.ToString(),
                LastPrice = t.LastPrice.ToString(),
            });
        }

        public TickerDto GetTicker(string baseCurrency, string quoteCurrency)
        {
            var t = ClientInstance.GetTicker(baseCurrency, quoteCurrency).Result;
            return new TickerDto
            {
                Symbol = t.Symbol,
                BaseCurrency = t.BaseCurrency,
                QuoteCurrency = t.QuoteCurrency,
                BaseVolume24h = t.Volume24h.ToString(),
                Volume7d = t.Volume7d,
                Change24h = t.Change24h.ToString(),
                Change7d = t.Change7d.ToString(),
                LastPrice = t.LastPrice.ToString(),
            };
        }

        public Rate GetRate(string baseCurrency, string quoteCurrency)
        {
            return ClientInstance.GetRate(baseCurrency, quoteCurrency).Result;
        }

        public IEnumerable<TradeDto> GetAllTrades(string baseCurrency, string quoteCurrency, int size = 100)
        {
            if (!string.IsNullOrEmpty(baseCurrency) && !string.IsNullOrEmpty(quoteCurrency))
            {
                var trades = ClientInstance.GetAllTrades(baseCurrency, quoteCurrency, size).Result;
                return trades.Select(t => new TradeDto
                {
                    BaseCurrency = t.BaseCurrency,
                    QuoteCurrency = t.QuoteCurrency,
                    Amount = t.Quantity.ToString(),
                    OrderTime = DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp).DateTime,
                    Id = t.Id,
                    Price = t.Price.ToString(),
                    //Type = t.IsMakerBuyer
                });
            }

            return new TradeDto[0];
        }

        public TradeDto GetLastTrade(string baseCurrency, string quoteCurrency)
        {
            return this.GetAllTrades(baseCurrency, quoteCurrency, 1).FirstOrDefault();
        }

        public IEnumerable<TradeDto> GetClientTrades(int page, int size)
        {
            var trades = ClientInstance.GetClientTrades(page, size).Result;
            return trades.Select(t => new TradeDto
            {
                BaseCurrency = t.BaseCurrency,
                QuoteCurrency = t.QuoteCurrency,
                Amount = t.Quantity.ToString(),
                OrderTime = DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp).DateTime,
                Id = t.Id,
                Price = t.Price.ToString(),
                //Type = t.IsMakerBuyer
            });
        }

        public IEnumerable<TradeDto> GetClientTradesPair(string baseCurrency, string quoteCurrency, long from = 0, int size = 20)
        {
            var trades = ClientInstance.GetClientTradesPair(baseCurrency, quoteCurrency, from, size).Result;
            return trades.Select(t => new TradeDto
            {
                BaseCurrency = t.BaseCurrency,
                QuoteCurrency = t.QuoteCurrency,
                Amount = t.Quantity.ToString(),
                OrderTime = DateTimeOffset.FromUnixTimeMilliseconds(t.Timestamp).DateTime,
                Id = t.Id,
                Price = t.Price.ToString(),
                //Type = t.IsMakerBuyer
            });
        }

        public IEnumerable<OrderDto> GetOrders(int size = 20)
        {
            var orders = ClientInstance.GetOrders(size).Result;
            return orders.Select(o => this.ConvertToOrderDto(o));
        }

        public IEnumerable<OrderDto> GetAllOrdersUntil(int days, int size)
        {
            var orders = GetOrdersUntil(days, size);
            return orders.Select(o => this.ConvertToOrderDto(o));
        }

        private IEnumerable<Order> GetOrdersUntil(int days, int size)
        {
            long from = DateTimeOffset.Now.AddDays(-1 * days).ToUnixTimeMilliseconds();
            var orders = ClientInstance.GetOrdersFrom(from, size).Result;

            //long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //while (true)
            //{
            //    var orders = ClientInstance.GetOrdersFrom(from, 1000).Result.ToList();
            //    if (!orders.Any())
            //    {
            //        break;
            //    }

            //    allorders.AddRange(orders);
            //    if (orders.Count == 1000 && orders.Last().Timestamp < now)
            //    {
            //        from = orders.Last().Timestamp + 1;
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}

            return orders;
        }

        public IEnumerable<OrderDto> GetActiveOrdersUntil(int days, int size)
        {
            var orders = GetOrdersUntil(days, size);
            var statuses = orders.Select(o => o.Status).Distinct();

            return orders.Where(o => o.Status == "ORDER_STATUS_PLACED").Select(o => this.ConvertToOrderDto(o));
        }

        public IEnumerable<OrderDto> GetCompletedOrders(string baseCurrency, string quoteCurrency, int size)
        {
            return new OrderDto[0];
        }

        public IEnumerable<OrderDto> GetCompletedOrdersUntil(int days, int size)
        {
            var orders = GetOrdersUntil(days, size);
            return orders.Where(o => o.Status != "ORDER_STATUS_PLACED").Select(o => this.ConvertToOrderDto(o));
        }

        public IEnumerable<OrderDto> GetAllOrdersForPair(string baseCurrency, string quoteCurrency, int page, int size)
        {
            var orders = ClientInstance.GetAllOrdersForPair(baseCurrency, quoteCurrency, page, size).Result;
            return orders.Select(o => this.ConvertToOrderDto(o));
        }

        public IEnumerable<OrderDto> GetActiveOrdersForPair(string baseCurrency, string quoteCurrency, int page = 0, int size = 20)
        {
            var orders = ClientInstance.GetActiveOrdersForPair(baseCurrency, quoteCurrency, page, size).Result;
            return orders.Select(o => this.ConvertToOrderDto(o));
        }

        public OrderResponse PlaceOrder(OrderCommand command)
        {
            return ClientInstance.PlaceOrder(command).Result;
        }

        public OrderResponse CancelOrder(OrderIdCommand command)
        {
            return ClientInstance.CancelOrder(command).Result;
        }

        public OrderDto GetOrder(OrderIdCommand command)
        {
            var order = ClientInstance.GetOrder(command).Result;
            OrderDto orderDto = ConvertToOrderDto(order);
            return orderDto;
        }

        private OrderDto ConvertToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                BaseCurrency = order.BaseCurrency,
                QuoteCurrency = order.QuoteCurrency,
                ClientOrderId = order.ClientOrderId,
                Condition = order.Condition.Replace("ORDER_CONDITION_", string.Empty),
                Cost = order.Cost,
                Filled = order.Filled,
                Price = order.Price,
                Quantity = order.Quantity,
                Side = order.Side.Replace("ORDER_SIDE_", string.Empty),
                Status = order.Status.Replace("ORDER_STATUS_", string.Empty),
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(order.Timestamp).DateTime,
                Trader = order.Trader,
                Type = order.Type.Replace("ORDER_TYPE_", string.Empty),
            };
        }

        public IEnumerable<BalanceDto> GetBalances(bool zeros = true)
        {
            var balances = ClientInstance.GetBalances(zeros).Result;
            return balances.Select(b => this.ConvertToBalanceDto(b));
        }

        public IEnumerable<TransferDto> GetAllTransfers(int page = 0)
        {
            var transfers = ClientInstance.GetTransfers(page).Result;
            return transfers.Content.Select(tr => this.ConvertToTransferDto(tr));
        }

        private TransferDto ConvertToTransferDto(Transfer transfer)
        {
            return new TransferDto
            {
                Id = transfer.Id,
                CodeRequired = transfer.CodeRequired,
                Currency = transfer.Currency,
                Direction = transfer.Direction,
                Fee = transfer.Fee,
                FromAccount = transfer.FromAccount,
                ToAccount = transfer.ToAccount,
                FromUser = transfer.FromUser,
                ToUser = transfer.ToUser,
                Method = transfer.Method,
                Recipient = transfer.Recipient,
                RejectReason = transfer.RejectReason,
                Sender = transfer.Sender,
                Status = transfer.Status,
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(transfer.Timestamp).DateTime,
                TransferringFunds = transfer.TransferringFunds,
                Type = transfer.Type,
                UsdValue = transfer.UsdValue,
            };
        }

        private BalanceDto ConvertToBalanceDto(Balance b)
        {
            return new BalanceDto
            {
                Id = b.Id,
                Available = b.Available,
                Blocked = b.Blocked,
                CurrencyId = b.CurrencyId,
                Status = b.Status.Replace("ACCOUNT_STATUS_", ""),
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(b.Timestampde).DateTime,
                Type = b.Type.Replace("ACCOUNT_TYPE_", ""),
            };
        }

        public CancelAllOrdersResponce CancelAllOrders(string baseCurrency, string quoteCurrency)
        {
            return ClientInstance.CancelAllOrders(baseCurrency, quoteCurrency).Result;
        }

        public LatokenUser GetUser()
        {
            return ClientInstance.GetUser().Result;
        }

        public BalanceDto GetWalletBalanceByCurrency(string currencyId)
        {
            var balance = ClientInstance.GetBalanceByType(currencyId, TypeOfAccount.ACCOUNT_TYPE_WALLET).Result;
            return this.ConvertToBalanceDto(balance);
        }

        public BalanceDto GetSpotBalanceByCurrency(string currencyId)
        {
            var balance = ClientInstance.GetBalanceByType(currencyId, TypeOfAccount.ACCOUNT_TYPE_SPOT).Result;
            return this.ConvertToBalanceDto(balance);
        }

        public Transaction SpotWithdraw(SpotTransferCommand command)
        {
            return ClientInstance.SpotWithdraw(command).Result;
        }

        public Transaction SpotDeposit(SpotTransferCommand command)
        {
            return ClientInstance.SpotDeposit(command).Result;
        }

        public Transaction TransferInternal(TransferCommand command)
        {
            return ClientInstance.TransferInternal(command).Result;
        }

        public TradingCompetitionUserPositionDto GetUserPositionForTradingCompetition(string competitionId)
        {
            var userPosition = ClientInstance.GetUserPositionForTradingCompetition(competitionId).Result;
            return this.ConvertToTradingCompetitionUserPositionDto(userPosition);
        }

        private TradingCompetitionUserPositionDto ConvertToTradingCompetitionUserPositionDto(TradingCompetitionUserPosition userPosition)
        {
            return new TradingCompetitionUserPositionDto
            {
                Nickname = userPosition.Nickname,
                Position = userPosition.Position,
                RewardValue = userPosition.RewardValue,
                TargetValue = userPosition.TargetValue,
            };
        }

        public bool IsReady()
        {
            return ClientInstance.IsReady();
        }
    }
}