using Latoken.Api.Client.Library;
using Latoken.Api.Client.Library.Commands;
using Latoken.Api.Client.Library.Dto.Rest;
using Latoken.Api.Client.Library.LA.Commands;

namespace LatokenMauiClient
{
    public interface IRestClient
    {

        //Todo remove this later
        ILARestClient ClientInstance { get; }

        IEnumerable<CurrencyDto> GetCurrencies();

        //CurrencyDto GetCurrency(string currencyId);

        IEnumerable<PairDto> GetAvailablePairs();

        IEnumerable<TradingCompetition> GetTradingCompetitions(int page = 0);

        IEnumerable<TradingCompetitionLeaderBoardPosition> GetTradingCompetitionLeaderBoard(string competitionId, int page = 0, int size=100);

        OrderBookDto GetOrderBook(CurrencyDto baseCurrency, CurrencyDto quoteCurrency, int limit);
        FeeScheme GetFeeSchemeForPair(string baseCurrency, string quoteCurrency);
        IEnumerable<TickerDto> GetTickers();
        TickerDto GetTicker(string baseCurrency, string quoteCurrency);
        Rate GetRate(string baseCurrency, string quoteCurrency);
        IEnumerable<TradeDto> GetAllTrades(string baseCurrency, string quoteCurrency, int size = 100);
        TradeDto GetLastTrade(string baseCurrency, string quoteCurrency);
        IEnumerable<TradeDto> GetClientTrades(int page, int size);
        IEnumerable<TradeDto> GetClientTradesPair(string baseCurrency, string quoteCurrency, long from = 0, int size = 20);
        IEnumerable<OrderDto> GetOrders(int size = 20);
        IEnumerable<OrderDto> GetActiveOrdersUntil(int days, int size);
        IEnumerable<OrderDto> GetCompletedOrders(string baseCurrency, string quoteCurrency, int size);
        IEnumerable<OrderDto> GetCompletedOrdersUntil(int days, int size);
        IEnumerable<OrderDto> GetAllOrdersUntil(int days, int size);
        IEnumerable<OrderDto> GetAllOrdersForPair(string baseCurrency, string quoteCurrency, int page, int size);
        IEnumerable<OrderDto> GetActiveOrdersForPair(string baseCurrency, string quoteCurrency, int page = 0, int size = 20);
        OrderResponse PlaceOrder(OrderCommand command);
        OrderResponse CancelOrder(OrderIdCommand command);
        OrderDto GetOrder(OrderIdCommand command);
        IEnumerable<BalanceDto> GetBalances(bool zeros = true);
        CancelAllOrdersResponce CancelAllOrders(string baseCurrency, string quoteCurrency);
        LatokenUser GetUser();
        BalanceDto GetWalletBalanceByCurrency(string currencyId);
        BalanceDto GetSpotBalanceByCurrency(string currencyId);

        /// <summary>
        /// Transfer from Spot to Wallet for the current user;
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Task.</returns>
        Transaction SpotWithdraw(SpotTransferCommand command);

        /// <summary>
        /// Transfer from Wallet to Spot for the current user;
        /// </summary>
        /// <param name="command"></param>
        /// <returns>Task.</returns>
        Transaction SpotDeposit(SpotTransferCommand command);

        /// <summary>
        /// Transfer assets from the wallet of one user account to another.
        /// </summary>
        /// <param name="command">The parameter that specifies the transfer parameters.</param>
        /// <returns>Task.</returns>
        Transaction TransferInternal(TransferCommand command);

        /// <summary>
        ///     Returns true if the REST client is ready to establish a connection
        /// </summary>        

        TradingCompetitionUserPosition GetUserPositionForTradingCompetition(string competitionId);

        bool IsReady();
    }
}