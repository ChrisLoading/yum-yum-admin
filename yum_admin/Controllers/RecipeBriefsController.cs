﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using yum_admin.Models;
using yum_admin.Models.DataTransferObject;
using yum_admin.Models.ViewModels;

namespace yum_admin.Controllers
{
    public class RecipeBriefsController : Controller
    {
        private readonly YumyumdbContext _context;

        public RecipeBriefsController(YumyumdbContext context)
        {
            _context = context;
        }

        // GET: RecipeBriefs
        public async Task<IActionResult> Index()
        {
            var yumyumdbContext = _context.RecipeBriefs.Include(r => r.Creator).Include(r => r.RecipeClass);
            return View(await yumyumdbContext.ToListAsync());
        }

        // GET: RecipeBriefs/Details/5
        public async Task<IActionResult> Details(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipeBrief = await _context.RecipeBriefs
                .Include(r => r.Creator)
                .Include(r => r.RecipeClass)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipeBrief == null)
            {
                return NotFound();
            }

            return View(recipeBrief);
        }

        // GET: RecipeBriefs/Create
        public IActionResult Create()
        {
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId");
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId");
            return View();
        }

        // POST: RecipeBriefs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RecipeId,RecipeName,RecipeClassId,FinishMinute,CreatorId,PersonQuantity,CreateDate,ClickTime")] RecipeBrief recipeBrief)
        {
            if (ModelState.IsValid)
            {
                _context.Add(recipeBrief);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", recipeBrief.CreatorId);
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId", recipeBrief.RecipeClassId);
            return View(recipeBrief);
        }

        // GET: RecipeBriefs/Edit/5
        public async Task<IActionResult> Edit(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipeBrief = await _context.RecipeBriefs.FindAsync(id);
            if (recipeBrief == null)
            {
                return NotFound();
            }
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", recipeBrief.CreatorId);
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId", recipeBrief.RecipeClassId);
            return View(recipeBrief);
        }

        // POST: RecipeBriefs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(short id, [Bind("RecipeId,RecipeName,RecipeClassId,FinishMinute,CreatorId,PersonQuantity,CreateDate,ClickTime")] RecipeBrief recipeBrief)
        {
            if (id != recipeBrief.RecipeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(recipeBrief);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RecipeBriefExists(recipeBrief.RecipeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatorId"] = new SelectList(_context.UserSecretInfos, "UserId", "UserId", recipeBrief.CreatorId);
            ViewData["RecipeClassId"] = new SelectList(_context.RecipeClasses, "RecipeClassId", "RecipeClassId", recipeBrief.RecipeClassId);
            return View(recipeBrief);
        }

        // GET: RecipeBriefs/Delete/5
        public async Task<IActionResult> Delete(short? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var recipeBrief = await _context.RecipeBriefs
                .Include(r => r.Creator)
                .Include(r => r.RecipeClass)
                .FirstOrDefaultAsync(m => m.RecipeId == id);
            if (recipeBrief == null)
            {
                return NotFound();
            }

            return View(recipeBrief);
        }

        // POST: RecipeBriefs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(short id)
        {
            var recipeBrief = await _context.RecipeBriefs.FindAsync(id);
            if (recipeBrief != null)
            {
                _context.RecipeBriefs.Remove(recipeBrief);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RecipeBriefExists(short id)
        {
            return _context.RecipeBriefs.Any(e => e.RecipeId == id);
        }
		public IActionResult Recipe()
		{
			// Step 1: 查詢最新的 RecipeRecords 和 RecipeRecordFields，並保持在資料庫內
			var latestRecipeRecords = _context.RecipeRecords
				.GroupBy(r => r.RecipeId)
				.Select(g => new
				{
					RecipeId = g.Key,
					LatestRecDate = g.Max(r => r.RecipeRecDate) // 取最大日期/最新日期
				});

			var latestRecipeFields = _context.RecipeRecordFields
				.GroupBy(rf => rf.RecipeId)
				.Select(g => new
				{
					RecipeId = g.Key,
					LatestField = g.Max(rf => rf.RecipeField) // 取最大欄位
				});

			// Step 2: 查詢所有需要的數據
			var recipesQuery = from brief in _context.RecipeBriefs
							   join record in _context.RecipeRecords
								   on brief.RecipeId equals record.RecipeId
							   join field in _context.RecipeRecordFields
								   on brief.RecipeId equals field.RecipeId
							   join state in _context.RecipeStates
								   on record.RecipeStatusCode equals state.RecipeStateCode
							   join userInfo in _context.UserSecretInfos
								   on brief.CreatorId equals userInfo.UserId
							   join latestRecord in latestRecipeRecords
								   on new { RecipeId = brief.RecipeId, RecipeRecDate = record.RecipeRecDate }
									  equals new { RecipeId = latestRecord.RecipeId, RecipeRecDate = latestRecord.LatestRecDate }
							   join latestField in latestRecipeFields
								   on new { RecipeId = brief.RecipeId, RecipeField = field.RecipeField }
									  equals new { RecipeId = latestField.RecipeId, RecipeField = latestField.LatestField }
							   select new RecipeViewModel
							   {
								   RecipeStateDescription = state.RecipeStateDescript,
								   RecipeStateCode = state.RecipeStateCode,
								   RecipeName = brief.RecipeName,
								   RecipeId = brief.RecipeId,
								   UserNickname = userInfo.UserNickname,
								   RecipeField = field.RecipeField,
								   RecipeRecDate = record.RecipeRecDate.ToDateTime(TimeOnly.MinValue)
							   };

			// Step 3: 加載查詢結果到內存
			var recipes = recipesQuery.ToList();

			return View(recipes);
		}

		public async Task<IActionResult> RecipeDetail(int recipeId)
		{
			// 查詢 RecipeBrief
			var recipeBrief = await _context.RecipeBriefs
				.Where(b => b.RecipeId == recipeId)
				.Select(b => new
				{
					b.RecipeName
				}).FirstOrDefaultAsync();

			if (recipeBrief == null)
			{
				return NotFound();
			}

			// 查詢 RecipeRecords（版本資料）
			var recipeVersions = await _context.RecipeRecords
				.Where(r => r.RecipeId == recipeId)
				.OrderByDescending(r => r.RecipeRecVersion)
				.Select(r => new RecipeVersionDetail
				{
					RecipeRecVersion = r.RecipeRecVersion,
					RecipeStatusCode = r.RecipeStatusCode,
					RecipeRecDate = r.RecipeRecDate.ToDateTime(TimeOnly.MinValue)
				}).ToListAsync();

			if (!recipeVersions.Any())
			{
				return NotFound();
			}

			// 查詢 RecipeFields 與 RecipeField，按版本分組，並與 RecipeRecords 匹配
			var recipeFieldsByVersion = await _context.RecipeRecordFields
				.Where(rf => rf.RecipeId == recipeId)
				.GroupBy(rf => rf.RecipeRecVersion)
				.Select(g => new RecipeFieldGroupedByVersion
				{
					RecipeRecVersion = g.Key,
					RecipeFields = g.Select(f => new RecipeFieldDetail
					{
						FieldShot = f.FieldShot,
						FieldDescript = f.FieldDescript,
						FieldCheck = f.FieldCheck,
						FieldComment = f.FieldComment,
						FieldName = _context.RecipeFields
							.Where(field => field.FieldId == f.RecipeField)
							.Select(field => field.FieldName).FirstOrDefault() ?? "Default Field Name"
					}).ToList()
				}).ToListAsync();

			// 確保每個版本的狀態碼從 RecipeRecords 對應到 RecipeFieldsByVersion
			foreach (var fieldGroup in recipeFieldsByVersion)
			{
				var matchingVersion = recipeVersions.FirstOrDefault(v => v.RecipeRecVersion == fieldGroup.RecipeRecVersion);
				if (matchingVersion != null)
				{
					fieldGroup.RecipeStatusCode = matchingVersion.RecipeStatusCode;
				}
			}

			// 組裝 ViewModel
			var viewModel = new RecipeDetailViewModel
			{
				RecipeId = recipeId,
				RecipeName = recipeBrief?.RecipeName ?? "Unknown Recipe",
				MaxVersion = recipeVersions.Max(r => r.RecipeRecVersion),
				PrevVersions = recipeVersions,
				RecipeFieldsByVersion = recipeFieldsByVersion ?? new List<RecipeFieldGroupedByVersion>(),
				RecipeRecDate = recipeVersions.FirstOrDefault()?.RecipeRecDate ?? DateTime.MinValue
			};

			return View(viewModel);
		}



		[Route("RecipeBriefs/Approved")]
		[HttpPost]
		public async Task<IActionResult> Approved(int recipeId, int recipeRecVersion)
		{
			Console.WriteLine($"Updating RecipeID: {recipeId}, RecipeRecVersion: {recipeRecVersion}");
			try
			{
				var recipeUpload = await _context.RecipeRecords.FirstOrDefaultAsync(
					r => r.RecipeId == recipeId && r.RecipeRecVersion == recipeRecVersion);

				if (recipeUpload == null)
				{
					Console.WriteLine("Record not found.");
					return Json(new { success = false, message = "食譜記錄未找到。" });
				}

				recipeUpload.RecipeStatusCode = 4;
				await _context.SaveChangesAsync();
				Console.WriteLine("Update successful.");
				return Content("食譜已通過審核！", "text/plain", System.Text.Encoding.UTF8);
				//return Json(new { success = true, message = "食譜已通過審核！" });
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
				return Json(new { success = false, message = "發生錯誤，請稍後再試。" });
			}
		}



		public IActionResult RecipeInfo()
        {
            return View();
        }
    }
}
