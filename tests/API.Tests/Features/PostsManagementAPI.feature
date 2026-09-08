@Category:Regression @Feature:PostsAPI @Risk:Medium @Layer:Api @Requirement:REQ-API-002
Feature: Posts Management API
  As a client application
  I want to manage blog posts via the API
  So that users can create and interact with content

  Background:
    Given the API is available at the configured base URL

  @Smoke
  Scenario: Retrieve all posts
    When I send a GET request to "/posts"
    Then the response status code should be 200
    And the response should contain multiple posts
    And each post should have "userId, id, title, body" fields

  @Risk:High
  Scenario: Get posts for a specific user
    When I send a GET request to "/posts" with query parameter "userId" equals "1"
    Then the response status code should be 200
    And all posts should belong to user "1"

  Scenario: Get a single post by ID
    When I send a GET request to "/posts/1"
    Then the response status code should be 200
    And the post should have id "1"
    And the post should have a non-empty title and body

  @Risk:Critical
  Scenario: Create a new post
    Given I have the following post data:
      | userId | title                    | body                          |
      | 1      | Test Post Title          | This is a test post body      |
    When I send a POST request to "/posts" with the post data
    Then the response status code should be 201
    And the response should contain the created post
    And the post should have an "id" field

  Scenario: Update a post completely
    Given a post exists with id "1"
    And I have updated post data:
      | userId | title         | body              |
      | 1      | Updated Title | Updated body text |
    When I send a PUT request to "/posts/1" with the updated data
    Then the response status code should be 200
    And the post "title" should be "Updated Title"
    And the post "body" should be "Updated body text"

  Scenario: Delete a post
    Given a post exists with id "1"
    When I send a DELETE request to "/posts/1"
    Then the response status code should be 200

  @Risk:High
  Scenario: Get comments for a post
    When I send a GET request to "/posts/1/comments"
    Then the response status code should be 200
    And the response should contain comments
    And each comment should have "postId, id, name, email, body" fields

  Scenario: Filter comments by post ID
    When I send a GET request to "/comments" with query parameter "postId" equals "1"
    Then the response status code should be 200
    And all comments should belong to post "1"

  @Risk:Medium
  Scenario: Verify post data integrity
    When I send a GET request to "/posts/1"
    Then the response status code should be 200
    And the post "title" should not be empty
    And the post "body" should not be empty
    And the post "userId" should be greater than 0

  Scenario: Create post with missing required fields
    Given I have incomplete post data:
      | title          |
      | Title Only     |
    When I send a POST request to "/posts" with the post data
    Then the response status code should be 400 or 422

  Scenario Outline: Retrieve posts by different IDs
    When I send a GET request to "/posts/<postId>"
    Then the response status code should be 200
    And the post should have id "<postId>"

    Examples:
      | postId |
      | 1      |
      | 5      |
      | 10     |
