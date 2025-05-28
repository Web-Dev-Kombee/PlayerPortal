using Microsoft.AspNetCore.Mvc;
using PlayerPortal.Data.BrokerRequests;
using PlayerPortal.Services;

namespace PlayerPortal.Controllers
{
    public class PlayerController : Controller
    {
        private readonly IPlayerServices _playerService;

        public PlayerController(IPlayerServices playerService)
        {
            _playerService = playerService;
        }

        public async Task<IActionResult> Index(string searchTerm, int page = 1, int pageSize = 10)
        {
            var (players, totalCount) = await _playerService.GetAllPlayers(searchTerm, page, pageSize);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            return View(players);
        }

        public async Task<IActionResult> Details(int id)
        {
            var player = await _playerService.GetPlayerById(id);
            if (player == null) return NotFound();
            return View(player);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlayerRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            request.ActionPerformedBy = 1;
            await _playerService.CreatePlayer(request);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var player = await _playerService.GetPlayerById(id);
            if (player == null) return NotFound();
            var request = _playerService.ToPlayerRequest(player);
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PlayerRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            request.ActionPerformedBy = 1;
            await _playerService.UpdatePlayer(id, request);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var player = await _playerService.GetPlayerById(id);
            if (player == null) return NotFound();
            return View(player);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _playerService.DeletePlayer(id);
            return RedirectToAction(nameof(Index));
        }
    }
}