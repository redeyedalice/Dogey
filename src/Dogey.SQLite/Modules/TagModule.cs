﻿using Discord.Commands;
using System.Linq;
using System.Threading.Tasks;

namespace Dogey.SQLite.Modules
{
    [Group("tag")]
    public class TagModule : ModuleBase<SocketCommandContext>
    {
        private TagDatabase _db;

        protected override void BeforeExecute()
        {
            _db = new TagDatabase();
        }

        protected override void AfterExecute()
        {
            _db.Dispose();
        }

        [Command, Priority(0)]
        [Remarks("Execute the specified tag.")]
        public async Task TagAsync([Remainder]string name)
        {
            var tag = await _db.GetTagAsync(Context, name.ToLower());

            if (tag == null)
            {
                var tags = await _db.FindTagsAsync(Context, name, 5);
                string related = string.Join(", ", tags.Select(x => x.Name));
                await ReplyAsync($"Could not find a tag like `{name}`. Did you mean:\n{related}");
                return;
            }

            await ReplyAsync($"{tag.Name}: {tag.Content}");
        }

        [Command("create"), Priority(10)]
        [Remarks("Create a new tag.")]
        public async Task CreateAsync(string name, [Remainder]string content)
        {
            await _db.CreateTagAsync(Context, name.ToLower(), content);
            await ReplyAsync(":thumbsup:");
        }

        [Command("edit"), Priority(10)]
        [Remarks("Edit and existing tag you own.")]
        public Task EditAsync(string name, [Remainder]string content)
        {
            return Task.CompletedTask;
        }

        [Command("delete"), Priority(10)]
        [Remarks("Delete an existing tag you own.")]
        public async Task DeleteAsync([Remainder]string name)
        {
            await _db.DeleteTagAsync(Context, name.ToLower());
            await ReplyAsync(":thumbsup:");
        }

        [Command("info"), Priority(10)]
        [Remarks("Get information about a tag.")]
        public Task InfoAsync([Remainder]string name)
        {
            var tag = _db.GetTagAsync(Context, name.ToLower());
            return Task.CompletedTask;
        }
    }
}
