using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Izm.Rumis.Application.Common
{
    public class CollectionHelper
    {
        /// <summary>
        /// Compare two collections and get the difference.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="equalityComparer"></param>
        /// <returns></returns>
        public static CollectionComparisonResult<T, T> Compare<T>(IEnumerable<T> left, IEnumerable<T> right, Func<T, T, bool> equalityComparer)
        {
            var notInLeft = right.Where(t => !left.Any(x => equalityComparer(t, x)));
            var notInRight = left.Where(t => !right.Any(x => equalityComparer(t, x)));

            return new CollectionComparisonResult<T, T>(notInLeft, notInRight);
        }

        /// <summary>
        /// Update the source collection with the new one.
        /// </summary>
        /// <typeparam name="T">Set entity type</typeparam>
        /// <typeparam name="TDto">Data transfer object type</typeparam>
        /// <param name="set">Target database set</param>
        /// <param name="source">Current entity collection</param>
        /// <param name="values">Target value collection</param>
        /// <param name="match">Function to compare source entity with update values</param>
        /// <param name="map">Function to map update values to an entity</param>
        public static void UpdateSet<T, TDto>(DbSet<T> set, ICollection<T> source, IEnumerable<TDto> values,
            Func<T, TDto, bool> match, Action<T, TDto> map) where T : class, new()
        {
            var matches = new List<T>();
            var toAdd = new List<TDto>();

            var newItems = values.ToList();
            var newItemCount = values.Count();

            for (int i = 0; i < newItemCount; i++)
            {
                var dto = newItems[i];

                // find a matching entity
                var matchingEntity = source.Where(t => match(t, dto)).FirstOrDefault();

                if (matchingEntity == null || matches.Contains(matchingEntity))
                {
                    // add
                    toAdd.Add(dto);
                }
                else
                {
                    // no action since entity is already in the database
                    // remember a matching entity to exclude it from matching again
                    matches.Add(matchingEntity);
                }
            }

            var unmatched = source.Where(t => !matches.Contains(t)).ToList();

            for (int i = 0; i < toAdd.Count; i++)
            {
                var dto = toAdd[i];
                var freeEntity = unmatched.Skip(i).Take(1).FirstOrDefault();

                if (freeEntity != null)
                {
                    // update
                    map(freeEntity, dto);
                    matches.Add(freeEntity);
                }
                else
                {
                    // add
                    var entity = new T();
                    map(entity, dto);
                    matches.Add(entity);
                    source.Add(entity);
                }
            }

            // remove all entities that didn't match
            var toRemove = source.Where(t => !matches.Contains(t)).ToList();

            foreach (var t in toRemove)
                source.Remove(t);

            set.RemoveRange(toRemove);
        }
    }

    public class CollectionComparisonResult<TLeft, TRight>
    {
        public CollectionComparisonResult(IEnumerable<TLeft> notInLeft, IEnumerable<TRight> notInRight)
        {
            this.NotInLeft = notInLeft;
            this.NotInRight = notInRight;
        }

        /// <summary>
        /// Items found in the right collection but missing in the left one
        /// </summary>
        public IEnumerable<TLeft> NotInLeft { get; private set; }

        /// <summary>
        /// Items found in the left collection but missing in the right one
        /// </summary>
        public IEnumerable<TRight> NotInRight { get; private set; }

        public bool CollectionsEqual => !NotInLeft.Any() && !NotInRight.Any();
    }
}
