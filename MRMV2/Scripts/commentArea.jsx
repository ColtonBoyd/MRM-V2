var CommentBox = React.createClass({
    getInitialState: function () {
        return { data: [] };
    },

    loadFromServer: function () {
        $.get(this.props.url, function (data) {
            this.setState({ data: data });
        }.bind(this));
    },


    handleCommentSubmit: function (comment) {
        $.ajax({
            type: "POST",
            url: "/Recipes/commentSubmit",
            data: { comment: comment.Text },
            dataType: 'json',
            success: function()
            {
                this.loadFromServer();
            }.bind(this)

        });


    },


    componentWillMount: function () {
        this.loadFromServer();
        window.setInterval(this.loadFromServer(), this.props.pollInterval);
    },


    render: function () {
        return (
          <div className="commentBox">
            <h1>Comments</h1>
            <CommentList data={this.state.data} />
            <CommentForm onCommentSubmit={this.handleCommentSubmit} />
          </div>
      );
    }
});

var CommentList = React.createClass({
    render: function () {
        var commentNodes = this.props.data.map(function (comment) {
            return (
              <Comment author={comment.Author} key={comment.Id}>
                  {comment.Text}
              </Comment>
            );
        });
        return (
          <div className="commentList">
              {commentNodes}
          </div>
    );
    }
});

var CommentForm = React.createClass({
    getInitialState: function() {
        return {author: '', text: ''};
    },
    handleAuthorChange: function(e) {
        this.setState({author: e.target.value});
    },
    handleTextChange: function(e) {
        this.setState({text: e.target.value});
    },
    handleSubmit: function(e) {
        e.preventDefault();
        var author = this.state.author.trim();
        var text = this.state.text.trim();
        if (!text || !author) {
            return;
        }
        this.props.onCommentSubmit({ Author: author, Text: text });
        this.setState({author: '', text: ''});
    },
    render: function() {
        return (
          <form className="commentForm" onSubmit={this.handleSubmit}>
          <input
          type="text"
          placeholder="Your name"
          value={this.state.author}
          onChange={this.handleAuthorChange}
        />
        <textarea placeholder="Add to this discussion" rows="3" disabled="disabled"></textarea>
        <input type="submit" value="Post" />
      </form>
    );
}
});

var Comment = React.createClass({

    render: function () {
        var md = new Remarkable();
        return (
          <div className="comment">
              md.render(this.props.children.toString());            
          </div>
      );
    }
});


ReactDOM.render(
  <CommentBox url="Recipes/getComments"  pollInterval="72500" />,
  document.getElementById('commentsArea')
);
