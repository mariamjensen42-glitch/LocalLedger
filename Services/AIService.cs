using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace LocalLedger.Services;

public class AIService
{
    private readonly IChatClient _chatClient;

    public AIService(IChatClient chatClient)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }

    public async Task<string> AnalyzeTransactionNote(string note)
    {
        App.Log($"分析备注: {note}");
        
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, 
                "你是一个智能记账助手。请分析用户输入的备注信息，判断这笔交易的类型（支出或收入）和最合适的分类。\n\n" +
                "支出分类选项：餐饮、交通、购物、娱乐、居住、医疗、其他\n" +
                "收入分类选项：工资、奖金、投资收益、其他\n\n" +
                "请只输出分类名称，不要输出其他内容。"),
            new ChatMessage(ChatRole.User, note)
        };

        try
        {
            var response = await _chatClient.GetResponseAsync(messages);
            return response.Text?.Trim() ?? "其他";
        }
        catch (Exception ex)
        {
            App.Log($"AI 调用失败: {ex.Message}");
            return "其他";
        }
    }

    public async Task<string> GenerateNoteSuggestion(decimal amount, string category)
    {
        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, 
                "你是一个智能记账助手。请根据金额和分类，生成一条简洁的备注建议。备注要简短自然，符合日常记账习惯。"),
            new ChatMessage(ChatRole.User, $"金额：{amount}，分类：{category}")
        };

        try
        {
            var response = await _chatClient.GetResponseAsync(messages);
            return response.Text?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            App.Log($"AI 调用失败: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> GetFinancialAdvice(int days = 30)
    {
        App.Log("GetFinancialAdvice 开始");
        
        var transactions = DataService.Instance.Transactions;
        if (!transactions.Any())
        {
            App.Log("没有交易数据");
            return "暂无足够的交易数据来生成建议。";
        }

        var recentTransactions = transactions
            .Where(t => t.Date >= DateTime.Now.AddDays(-days))
            .ToList();

        var totalIncome = recentTransactions
            .Where(t => t.Type == Models.TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpense = recentTransactions
            .Where(t => t.Type == Models.TransactionType.Expense)
            .Sum(t => t.Amount);

        var topExpenseCategories = recentTransactions
            .Where(t => t.Type == Models.TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new { Category = g.Key, Amount = g.Sum(t => t.Amount) })
            .OrderByDescending(g => g.Amount)
            .Take(3)
            .ToList();

        var context = $"最近{days}天数据：\n" +
                      $"总收入：{totalIncome}\n" +
                      $"总支出：{totalExpense}\n" +
                      $"结余：{totalIncome - totalExpense}\n" +
                      $"支出Top3分类：{string.Join(", ", topExpenseCategories.Select(c => $"{c.Category}({c.Amount})"))}";

        App.Log("准备发送请求到 DeepSeek");

        var messages = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, 
                "你是一个专业的财务顾问。请根据用户的交易数据，提供简洁实用的财务建议。建议要具体、可操作，不超过3条。"),
            new ChatMessage(ChatRole.User, context)
        };

        try
        {
            App.Log("调用 _chatClient.GetResponseAsync...");
            var response = await _chatClient.GetResponseAsync(messages);
            var text = response.Text?.Trim() ?? "暂无足够的交易数据来生成建议。";
            App.Log($"收到响应: {text}");
            return text;
        }
        catch (Exception ex)
        {
            App.Log($"异常: {ex.GetType().Name}: {ex.Message}");
            return $"获取财务建议失败: {ex.Message}";
        }
    }
}
