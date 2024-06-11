# b-tree

[B-tree](https://en.wikipedia.org/wiki/B-tree) implementation.

## About

A B-tree is essentially a data structure that allows you to search for things quickly. It's similar to a binary search tree, but in a more general form.

The purpose of this project is to understand B-trees better by implementing one. It supports basic insert, find and delete operations. There's also a small command line application that allows you to interactively insert and delete entries and print the result.

### Code overview

- BTrees.Lib contains the B-tree implementation
- BTrees.Test contains tests
- BTrees.Dev is for manual tests by visualizing the tree (example below)

```
3
|__1
   |__0
   |__2
|__5,7
   |__4
   |__6
   |__8,9
```

## Resources

Check out [this excellent visualization](https://www.cs.usfca.edu/~galles/visualization/BTree.html) by David Galles. You can find more of them for other algorithms [here](https://www.cs.usfca.edu/~galles/visualization/Algorithms.html).

For whiteboard explanations, have a look at Jenny's Lectures CS IT on YouTube (see [insertion](https://www.youtube.com/watch?v=aNU9XYYCHu8) and [deletion](https://www.youtube.com/watch?v=GKa_t7fF8o0)).
